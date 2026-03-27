using System.Text;
using System.Xml;
using System.Xml.Schema;
using System.Xml.XPath;
using Schematron.Formatters;

namespace Schematron;

/// <summary>
/// Performs validation of Schematron elements and schemas.
/// </summary>
/// <remarks>
/// Can handle either standalone or embedded schematron schemas. If the schematron
/// is embedded in an XML Schema, the input document is validated against both at
/// the same time.
/// </remarks>
public class Validator
{
    #region Fields & Ctors

    XmlSchemaSet _xmlschemas = new XmlSchemaSet();
    SchemaCollection _schematrons = new SchemaCollection();
    EvaluationContextBase _evaluationctx;
    NavigableType _navtype = NavigableType.XPathDocument;

    StringBuilder? _errors;
    bool _haserrors;

    /// <summary>
    /// Initializes a new instance of the class.
    /// </summary>
    public Validator()
    {
        _evaluationctx = CreateContext();
        _evaluationctx.Formatter = Config.DefaultFormatter;
    }

    /// <summary>
    /// Initializes a new instance of the class, using the specified output format for error messages.
    /// </summary>
    /// <param name="format">Output format of error messages.</param>
    public Validator(OutputFormatting format) : this()
    {
        InitValidator(format, NavigableType.Default);
    }

    /// <summary>
    /// Initializes a new instance of the class, using the specified return type.
    /// </summary>
    /// <param name="type">The <see cref="IXPathNavigable"/> type to use for validation and return type.</param>
    public Validator(NavigableType type) : this()
    {
        InitValidator(OutputFormatting.Default, type);
    }

    /// <summary>
    /// Initializes a new instance of the class, using the specified options.
    /// </summary>
    /// <param name="format">Output format of error messages.</param>
    /// <param name="type">The <see cref="IXPathNavigable"/> type to use for validation and return type.</param>
    public Validator(OutputFormatting format, NavigableType type) : this()
    {
        InitValidator(format, type);
    }

    #endregion Fields & Ctors

    /// <summary>
    /// Initializes the validator with the options received from the constructor overloads.
    /// </summary>
    /// <param name="format">Output format of error messages.</param>
    /// <param name="type">The <see cref="IXPathNavigable"/> type to use for validation and return type.</param>
    void InitValidator(OutputFormatting format, NavigableType type)
    {
        if (!Enum.IsDefined(typeof(OutputFormatting), format))
            throw new ArgumentException("Invalid type.", "type");

        switch (format)
        {
            case OutputFormatting.Boolean:
                _evaluationctx.Formatter = new BooleanFormatter();
                break;
            case OutputFormatting.Log:
            case OutputFormatting.Default:
                _evaluationctx.Formatter = new LogFormatter();
                break;
            case OutputFormatting.Simple:
                _evaluationctx.Formatter = new SimpleFormatter();
                break;
            case OutputFormatting.XML:
                _evaluationctx.Formatter = new XmlFormatter();
                break;
        }

        if (!Enum.IsDefined(typeof(NavigableType), type))
            throw new ArgumentException("Invalid type.", "type");

        // If type is Default, set it to XPathDocument.
        _navtype = (type != NavigableType.Default) ? type : NavigableType.XPathDocument;
    }

    #region Overridable Factory Methods
    /// <summary>Creates the evaluation context to use.</summary>
    /// <remarks>
    /// Inheritors can override this method should they want to 
    /// use a different strategy for node traversal and evaluation
    /// against the source file.
    /// </remarks>
    protected virtual EvaluationContextBase CreateContext()
    {
        return new SyncEvaluationContext();
    }
    #endregion

    #region Properties
    /// <summary />
    public EvaluationContextBase Context
    {
        get { return _evaluationctx; }
        set { _evaluationctx = value; }
    }

    /// <summary />
    public IFormatter Formatter
    {
        get { return _evaluationctx.Formatter; }
        set { _evaluationctx.Formatter = value; }
    }

    /// <summary />
    public NavigableType ReturnType
    {
        get { return _navtype; }
        set
        {
            if (!Enum.IsDefined(typeof(NavigableType), value))
                throw new ArgumentException("NavigableType value is not defined.");
            _navtype = value;
        }
    }

    /// <summary />
    public string Phase
    {
        get { return _evaluationctx.Phase; }
        set { _evaluationctx.Phase = value; }
    }

    /// <summary>
    /// Exposes the schematron schemas to use for validation.
    /// </summary>
    public SchemaCollection Schemas
    {
        get { return _schematrons; }
    }

    /// <summary>
    /// Exposes the XML schemas to use for validation.
    /// </summary>
    public XmlSchemaSet XmlSchemas
    {
        get { return _xmlschemas; }
    }

    #endregion

    #region AddSchema overloads
    /// <summary>
    /// Adds an XML Schema to the collection to use for validation.
    /// </summary>
    public void AddSchema(XmlSchema schema)
    {
        _xmlschemas.Add(schema);
    }

    /// <summary>
    /// Adds a Schematron schema to the collection to use for validation.
    /// </summary>
    public void AddSchema(Schema schema)
    {
        _schematrons.Add(schema);
    }

    /// <summary>
    /// Adds a set of XML Schemas to the collection to use for validation.
    /// </summary>
    public void AddSchemas(XmlSchemaSet schemas)
    {
        _xmlschemas.Add(schemas);
    }

    /// <summary>
    /// Adds a set of Schematron schemas to the collection to use for validation.
    /// </summary>
    public void AddSchemas(SchemaCollection schemas)
    {
        _schematrons.AddRange(schemas);
    }

    /// <summary>
    /// Adds a schema to the collection to use for validation from the specified URL.
    /// </summary>
    public void AddSchema(string uri)
    {
        using (var fs = new FileStream(uri, FileMode.Open, FileAccess.Read, FileShare.Read))
        {
            AddSchema(new XmlTextReader(fs));
        }
    }

    /// <summary>
    /// Adds a schema to the collection to use for validation.
    /// </summary>
    public void AddSchema(TextReader reader)
    {
        AddSchema(new XmlTextReader(reader));
    }

    /// <summary>
    /// Adds a schema to the collection to use for validation.
    /// </summary>
    public void AddSchema(Stream input)
    {
        AddSchema(new XmlTextReader(input));
    }

    /// <summary>
    /// Adds a schema to the collection to use for validation.
    /// </summary>
    /// <remarks>Processing takes place here.</remarks>
    public void AddSchema(XmlReader reader)
    {
        if (reader.MoveToContent() == XmlNodeType.None) throw new BadSchemaException("No information found to read");

        // Determine type of schema received.
        bool standalone = (reader.NamespaceURI == Schema.Namespace);
        bool wxs = (reader.NamespaceURI == XmlSchema.Namespace);

        // The whole schema must be read first to preserve the state for later.
        string state = reader.ReadOuterXml();
        var r = new StringReader(state);

        if (wxs)
        {
            _haserrors = false;
            _errors = new StringBuilder();

            var xs = XmlSchema.Read(new XmlTextReader(r, reader.NameTable), new ValidationEventHandler(OnValidation));

            var set = new XmlSchemaSet();
            set.Add(xs);

            if (!set.IsCompiled)
            {
                set.Compile();
            }

            if (_haserrors) throw new BadSchemaException(_errors.ToString());

            _xmlschemas.Add(xs);
        }

        //Schemas wouldn't be too big, so they are loaded in an XmlDocument for Schematron validation, so that
        //inner XML elements in messages, etc. are available. So we commented the following lines.
        //r = new StringReader(state);
        //XPathNavigator nav = new XPathDocument(new XmlTextReader(r, reader.NameTable)).CreateNavigator();
        var doc = new XmlDocument(reader.NameTable);
        doc.LoadXml(state);
        XPathNavigator nav = doc.CreateNavigator();
        _evaluationctx.Source = nav;

        if (standalone)
            PerformValidation(Config.FullSchematron);
        else
            PerformValidation(Config.EmbeddedSchematron);

        if (_evaluationctx.HasErrors)
            throw new BadSchemaException(_evaluationctx.Messages.ToString());

        var sch = new Schema();
        sch.Load(nav);
        _schematrons.Add(sch);
        _errors = null;
    }


    #region WORK IN PROGRESS :: The need the for the signature AddSchema(string targetNamespace, string schemaUri) comes from resolving imported (schemaLocation hinted) partial schemas

    bool TryAddXmlSchema(
        string targetNamespace,
        string schemaUri,
        XmlSchemaSet schemaSet,
        Action<object, ValidationEventArgs> validationHandler,
        out string? xmlContent,
        out XmlNameTable? nameTable,
        out string? namespaceUri)
    {
        xmlContent = null;
        nameTable = null;
        namespaceUri = null;

        using (var reader = XmlReader.Create(schemaUri))
        {
            if (reader.MoveToContent() == XmlNodeType.None)
            {
                throw new BadSchemaException("No information found to read");
            }

            nameTable = reader.NameTable;
            namespaceUri = reader.NamespaceURI;
            xmlContent = reader.ReadOuterXml();

            if (!IsStandardSchema(namespaceUri))
                return false;

            if (_errors == null)
                _errors = new StringBuilder();

            var set = new XmlSchemaSet();
            set.XmlResolver = new XmlUrlResolver();
            set.Add(targetNamespace, new Uri(Path.GetFullPath(schemaUri)).AbsoluteUri);

            if (!set.IsCompiled)
                set.Compile();

            if (_haserrors) throw new BadSchemaException(_errors.ToString());

            foreach (XmlSchema s in set.Schemas())
                schemaSet.Add(s);
        }

        return true;
    }

    bool IsStandaloneSchematron(string? namespaceUri) => namespaceUri == Schema.Namespace;

    bool IsStandardSchema(string? namespaceUri) => namespaceUri == XmlSchema.Namespace;

    /// <summary>
    /// Adds a schema to the collection to use for validation.
    /// </summary>
    /// <remarks>Validation takes place here.</remarks>
    public void AddSchema(string targetNamespace, string schemaUri)
    {
        TryAddXmlSchema(targetNamespace, schemaUri, _xmlschemas, OnValidation, out var xmlContent, out var nameTable, out var namespaceUri);

        var doc = new XmlDocument(nameTable!);
        doc.LoadXml(xmlContent!);

        XPathNavigator nav = doc.CreateNavigator();
        _evaluationctx.Source = nav;

        if (IsStandaloneSchematron(namespaceUri))
            PerformValidation(Config.FullSchematron);
        else
            PerformValidation(Config.EmbeddedSchematron);

        if (_evaluationctx.HasErrors)
            throw new BadSchemaException(_evaluationctx.Messages.ToString());

        var sch = new Schema();
        sch.Load(nav);

        _schematrons.Add(sch);
        _errors = null;
    }

    #endregion


    #endregion

    #region Validation Methods
    /// <summary>
    /// Performs Schematron-only validation.
    /// </summary>
    /// <remarks>
    /// Even when <see cref="XmlDocument"/> implements IXPathNavigable, WXS
    /// validation can't be performed once it has been loaded becasue a 
    /// validating reader has to be used.
    /// </remarks>
    /// <exception cref="ValidationException">
    /// The document is invalid with respect to the loaded schemas.
    /// </exception>
    public void ValidateSchematron(IXPathNavigable source)
    {
        ValidateSchematron(source.CreateNavigator());
    }

    /// <summary>
    /// Performs Schematron-only validation.
    /// </summary>
    /// <exception cref="ValidationException">
    /// The document is invalid with respect to the loaded schemas.
    /// </exception>
    public void ValidateSchematron(XPathNavigator file)
    {
        _errors = new StringBuilder();
        _evaluationctx.Source = file;

        foreach (Schema sch in _schematrons)
        {
            PerformValidation(sch);
            if (_evaluationctx.HasErrors)
            {
                _haserrors = true;
                _errors.Append(_evaluationctx.Messages.ToString());
            }
        }

        if (_haserrors)
        {
            _evaluationctx.Formatter.Format(_errors);
            throw new ValidationException(_errors.ToString());
        }

        if (_haserrors) throw new ValidationException(_errors.ToString());
    }

    /// <summary>Performs validation of the document at the specified URI.</summary>
    /// <param name="uri">The document location.</param>
    /// <exception cref="ValidationException">
    /// The document is invalid with respect to the loaded schemas.
    /// </exception>
    /// <returns>The loaded <see cref="IXPathNavigable"/> instance.</returns>
    public IXPathNavigable Validate(string uri)
    {
        using (var fs = new FileStream(uri, FileMode.Open, FileAccess.Read, FileShare.Read))
        {
            return Validate(new XmlTextReader(fs));
        }
    }

    /// <summary>Performs validation of the document using the specified reader.</summary>
    /// <param name="reader">The reader pointing to the document to validate.</param>
    /// <returns>The loaded <see cref="IXPathNavigable"/> instance.</returns>
    /// <exception cref="ValidationException">
    /// The document is invalid with respect to the loaded schemas.
    /// </exception>
    /// <returns>The loaded <see cref="IXPathNavigable"/> instance.</returns>
    public IXPathNavigable Validate(TextReader reader)
    {
        return Validate(new XmlTextReader(reader));
    }

    /// <summary>Performs validation of the document using the specified stream.</summary>
    /// <param name="input">The stream with the document to validate.</param>
    /// <exception cref="ValidationException">
    /// The document is invalid with respect to the loaded schemas.
    /// </exception>
    /// <returns>The loaded <see cref="IXPathNavigable"/> instance.</returns>
    public IXPathNavigable Validate(Stream input)
    {
        return Validate(new XmlTextReader(input));
    }

    /// <summary>Performs validation of the document using the received reader.</summary>
    /// <remarks>Where the actual work takes place</remarks>
    /// <param name="reader">The reader pointing to the document to validate.</param>
    /// <exception cref="ValidationException">
    /// The document is invalid with respect to the loaded schemas.
    /// </exception>
    /// <returns>The loaded <see cref="IXPathNavigable"/> instance.</returns>
    public IXPathNavigable Validate(XmlReader reader)
    {
        _errors = new StringBuilder();

        bool hasxml = false;
        StringBuilder? xmlerrors = null;
        bool hassch = false;
        StringBuilder? scherrors = null;

        var settings = new XmlReaderSettings
        {
            ValidationType = ValidationType.Schema,
        };
        settings.ValidationEventHandler += OnValidation;

        foreach (XmlSchema xsd in _xmlschemas.Schemas())
            settings.Schemas.Add(xsd);

        var r = XmlReader.Create(reader, settings);

        IXPathNavigable navdoc;
        XPathNavigator nav;

        try
        {
            if (_navtype == NavigableType.XmlDocument)
            {
                navdoc = new XmlDocument(r.NameTable);
                ((XmlDocument)navdoc).Load(r);
            }
            else
            {
                navdoc = new XPathDocument(r);
            }
        }
        finally
        {
            reader.Close();
        }

        nav = navdoc.CreateNavigator();

        if (_haserrors)
        {
            _evaluationctx.Formatter.Format(r.Settings.Schemas, _errors);
            _evaluationctx.Formatter.Format(r, _errors);
            hasxml = true;
            xmlerrors = _errors;
        }

        _evaluationctx.Source = nav;

        // Reset shared variables
        _haserrors = false;
        _errors = new StringBuilder();

        foreach (Schema sch in _schematrons)
        {
            PerformValidation(sch);
            if (_evaluationctx.HasErrors)
            {
                _haserrors = true;
                _errors.Append(_evaluationctx.Messages.ToString());
            }
        }

        if (_haserrors)
        {
            _evaluationctx.Formatter.Format(_schematrons, _errors);
            hassch = true;
            scherrors = _errors;
        }

        _errors = new StringBuilder();
        if (hasxml) _errors.Append(xmlerrors!.ToString());
        if (hassch) _errors.Append(scherrors!.ToString());

        if (hasxml || hassch)
        {
            _evaluationctx.Formatter.Format(_errors);
            throw new ValidationException(_errors.ToString());
        }

        return navdoc;
    }
    #endregion

    void PerformValidation(Schema schema)
    {
        _evaluationctx.Schema = schema;
        _evaluationctx.Start();
    }

    void OnValidation(object sender, ValidationEventArgs e)
    {
        _haserrors = true;
        _evaluationctx.Formatter.Format(e, _errors!);
    }
}

