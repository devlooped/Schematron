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
    readonly XmlSchemaSet xmlschemas = new();
    readonly SchemaCollection schematrons = [];
    NavigableType navtype = NavigableType.XPathDocument;

    StringBuilder errors = new();
    bool haserrors;

    /// <summary>
    /// Initializes a new instance of the class.
    /// </summary>
    public Validator()
    {
        Context = CreateContext();
        Context.Formatter = Config.DefaultFormatter;
    }

    /// <summary>
    /// Initializes a new instance of the class, using the specified output format for error messages.
    /// </summary>
    /// <param name="format">Output format of error messages.</param>
    public Validator(OutputFormatting format) : this() => InitValidator(format, NavigableType.Default);

    /// <summary>
    /// Initializes a new instance of the class, using the specified return type.
    /// </summary>
    /// <param name="type">The <see cref="IXPathNavigable"/> type to use for validation and return type.</param>
    public Validator(NavigableType type) : this() => InitValidator(OutputFormatting.Default, type);

    /// <summary>
    /// Initializes a new instance of the class, using the specified options.
    /// </summary>
    /// <param name="format">Output format of error messages.</param>
    /// <param name="type">The <see cref="IXPathNavigable"/> type to use for validation and return type.</param>
    public Validator(OutputFormatting format, NavigableType type) : this() => InitValidator(format, type);

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
                Context.Formatter = new BooleanFormatter();
                break;
            case OutputFormatting.Log:
            case OutputFormatting.Default:
                Context.Formatter = new LogFormatter();
                break;
            case OutputFormatting.Simple:
                Context.Formatter = new SimpleFormatter();
                break;
            case OutputFormatting.XML:
                Context.Formatter = new XmlFormatter();
                break;
        }

        if (!Enum.IsDefined(typeof(NavigableType), type))
            throw new ArgumentException("Invalid type.", "type");

        // If type is Default, set it to XPathDocument.
        navtype = (type != NavigableType.Default) ? type : NavigableType.XPathDocument;
    }

    /// <summary>Creates the evaluation context to use.</summary>
    /// <remarks>
    /// Inheritors can override this method should they want to 
    /// use a different strategy for node traversal and evaluation
    /// against the source file.
    /// </remarks>
    protected virtual EvaluationContextBase CreateContext() => new SyncEvaluationContext();

    /// <summary />
    public EvaluationContextBase Context { get; set; }

    /// <summary />
    public IFormatter Formatter
    {
        get => Context.Formatter;
        set => Context.Formatter = value;
    }

    /// <summary />
    public NavigableType ReturnType
    {
        get { return navtype; }
        set
        {
            if (!Enum.IsDefined(typeof(NavigableType), value))
                throw new ArgumentException("NavigableType value is not defined.");
            navtype = value;
        }
    }

    /// <summary />
    public string Phase
    {
        get => Context.Phase;
        set => Context.Phase = value;
    }

    /// <summary>
    /// Exposes the schematron schemas to use for validation.
    /// </summary>
    public SchemaCollection Schemas => schematrons;

    /// <summary>
    /// Exposes the XML schemas to use for validation.
    /// </summary>
    public XmlSchemaSet XmlSchemas => xmlschemas;

    /// <summary>
    /// Adds an XML Schema to the collection to use for validation.
    /// </summary>
    public void AddSchema(XmlSchema schema) => xmlschemas.Add(schema);

    /// <summary>
    /// Adds a Schematron schema to the collection to use for validation.
    /// </summary>
    public void AddSchema(Schema schema) => schematrons.Add(schema);

    /// <summary>
    /// Adds a set of XML Schemas to the collection to use for validation.
    /// </summary>
    public void AddSchemas(XmlSchemaSet schemas) => xmlschemas.Add(schemas);

    /// <summary>
    /// Adds a set of Schematron schemas to the collection to use for validation.
    /// </summary>
    public void AddSchemas(SchemaCollection schemas) => schematrons.AddRange(schemas);

    /// <summary>
    /// Adds a schema to the collection to use for validation from the specified URL.
    /// </summary>
    public void AddSchema(string uri)
    {
        using var fs = new FileStream(uri, FileMode.Open, FileAccess.Read, FileShare.Read);
        AddSchema(new XmlTextReader(fs));
    }

    /// <summary>
    /// Adds a schema to the collection to use for validation.
    /// </summary>
    public void AddSchema(TextReader reader) => AddSchema(new XmlTextReader(reader));

    /// <summary>
    /// Adds a schema to the collection to use for validation.
    /// </summary>
    public void AddSchema(Stream input) => AddSchema(new XmlTextReader(input));

    /// <summary>
    /// Adds a schema to the collection to use for validation.
    /// </summary>
    /// <remarks>Processing takes place here.</remarks>
    public void AddSchema(XmlReader reader)
    {
        if (reader.MoveToContent() == XmlNodeType.None)
            throw new BadSchemaException("No information found to read");

        // Determine type of schema received.
        var standalone = Schema.IsSchematronNamespace(reader.NamespaceURI);
        var wxs = (reader.NamespaceURI == XmlSchema.Namespace);

        // The whole schema must be read first to preserve the state for later.
        var state = reader.ReadOuterXml();
        var r = new StringReader(state);

        if (wxs)
        {
            haserrors = false;
            errors.Clear();

            var xs = XmlSchema.Read(new XmlTextReader(r, reader.NameTable), new ValidationEventHandler(OnValidation));

            var set = new XmlSchemaSet();
            set.Add(xs);

            if (!set.IsCompiled)
            {
                set.Compile();
            }

            if (haserrors) throw new BadSchemaException(errors.ToString());

            xmlschemas.Add(xs);
        }

        //Schemas wouldn't be too big, so they are loaded in an XmlDocument for Schematron validation, so that
        //inner XML elements in messages, etc. are available. So we commented the following lines.
        //r = new StringReader(state);
        //XPathNavigator nav = new XPathDocument(new XmlTextReader(r, reader.NameTable)).CreateNavigator();
        var doc = new XmlDocument(reader.NameTable);
        doc.LoadXml(state);
        var nav = doc.CreateNavigator();
        Context.Source = nav;

        if (standalone)
            PerformValidation(Config.FullSchematron);
        else
            PerformValidation(Config.EmbeddedSchematron);

        if (Context.HasErrors)
            throw new BadSchemaException(Context.Messages.ToString());

        var sch = new Schema();
        sch.Load(nav);
        schematrons.Add(sch);
        errors.Clear();
    }


    #region WORK IN PROGRESS:: The need the for the signature AddSchema(string targetNamespace, string schemaUri) comes from resolving imported (schemaLocation hinted) partial schemas

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

        using var reader = XmlReader.Create(schemaUri);
        if (reader.MoveToContent() == XmlNodeType.None)
            throw new BadSchemaException("No information found to read");

        nameTable = reader.NameTable;
        namespaceUri = reader.NamespaceURI;
        xmlContent = reader.ReadOuterXml();

        if (!IsStandardSchema(namespaceUri))
            return false;

        errors.Clear();

        var set = new XmlSchemaSet
        {
            XmlResolver = new XmlUrlResolver()
        };
        set.Add(targetNamespace, new Uri(Path.GetFullPath(schemaUri)).AbsoluteUri);

        if (!set.IsCompiled)
            set.Compile();

        if (haserrors) throw new BadSchemaException(errors.ToString());

        foreach (XmlSchema s in set.Schemas())
            schemaSet.Add(s);

        return true;
    }

    bool IsStandaloneSchematron(string? namespaceUri) => Schema.IsSchematronNamespace(namespaceUri);

    bool IsStandardSchema(string? namespaceUri) => namespaceUri == XmlSchema.Namespace;

    /// <summary>
    /// Adds a schema to the collection to use for validation.
    /// </summary>
    /// <remarks>Validation takes place here.</remarks>
    public void AddSchema(string targetNamespace, string schemaUri)
    {
        TryAddXmlSchema(targetNamespace, schemaUri, xmlschemas, OnValidation, out var xmlContent, out var nameTable, out var namespaceUri);

        var doc = new XmlDocument(nameTable!);
        doc.LoadXml(xmlContent!);

        var nav = doc.CreateNavigator();
        Context.Source = nav;

        if (IsStandaloneSchematron(namespaceUri))
            PerformValidation(Config.FullSchematron);
        else
            PerformValidation(Config.EmbeddedSchematron);

        if (Context.HasErrors)
            throw new BadSchemaException(Context.Messages.ToString());

        var sch = new Schema();
        sch.Load(nav);

        schematrons.Add(sch);
        errors.Clear();
    }

    #endregion


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
    public void ValidateSchematron(IXPathNavigable source) => ValidateSchematron(source.CreateNavigator());

    /// <summary>
    /// Performs Schematron-only validation.
    /// </summary>
    /// <exception cref="ValidationException">
    /// The document is invalid with respect to the loaded schemas.
    /// </exception>
    public void ValidateSchematron(XPathNavigator file)
    {
        errors.Clear();
        Context.Source = file;

        foreach (var sch in schematrons)
        {
            PerformValidation(sch);
            if (Context.HasErrors)
            {
                haserrors = true;
                errors.Append(Context.Messages.ToString());
            }
        }

        if (haserrors)
        {
            Context.Formatter.Format(errors);
            throw new ValidationException(errors.ToString());
        }

        if (haserrors) throw new ValidationException(errors.ToString());
    }

    /// <summary>Performs validation of the document at the specified URI.</summary>
    /// <param name="uri">The document location.</param>
    /// <exception cref="ValidationException">
    /// The document is invalid with respect to the loaded schemas.
    /// </exception>
    /// <returns>The loaded <see cref="IXPathNavigable"/> instance.</returns>
    public IXPathNavigable Validate(string uri)
    {
        using var fs = new FileStream(uri, FileMode.Open, FileAccess.Read, FileShare.Read);
        return Validate(new XmlTextReader(fs));
    }

    /// <summary>Performs validation of the document using the specified reader.</summary>
    /// <param name="reader">The reader pointing to the document to validate.</param>
    /// <returns>The loaded <see cref="IXPathNavigable"/> instance.</returns>
    /// <exception cref="ValidationException">
    /// The document is invalid with respect to the loaded schemas.
    /// </exception>
    /// <returns>The loaded <see cref="IXPathNavigable"/> instance.</returns>
    public IXPathNavigable Validate(TextReader reader) => Validate(new XmlTextReader(reader));

    /// <summary>Performs validation of the document using the specified stream.</summary>
    /// <param name="input">The stream with the document to validate.</param>
    /// <exception cref="ValidationException">
    /// The document is invalid with respect to the loaded schemas.
    /// </exception>
    /// <returns>The loaded <see cref="IXPathNavigable"/> instance.</returns>
    public IXPathNavigable Validate(Stream input) => Validate(new XmlTextReader(input));

    /// <summary>Performs validation of the document using the received reader.</summary>
    /// <remarks>Where the actual work takes place</remarks>
    /// <param name="reader">The reader pointing to the document to validate.</param>
    /// <exception cref="ValidationException">
    /// The document is invalid with respect to the loaded schemas.
    /// </exception>
    /// <returns>The loaded <see cref="IXPathNavigable"/> instance.</returns>
    public IXPathNavigable Validate(XmlReader reader)
    {
        errors.Clear();

        var hasxml = false;
        string? xmlErrorText = null;
        var hassch = false;
        string? schErrorText = null;

        var settings = new XmlReaderSettings
        {
            ValidationType = ValidationType.Schema,
        };
        settings.ValidationEventHandler += OnValidation;

        foreach (XmlSchema xsd in xmlschemas.Schemas())
            settings.Schemas.Add(xsd);

        var r = XmlReader.Create(reader, settings);

        IXPathNavigable navdoc;
        XPathNavigator nav;

        try
        {
            if (navtype == NavigableType.XmlDocument)
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

        if (haserrors)
        {
            Context.Formatter.Format(r.Settings.Schemas, errors);
            Context.Formatter.Format(r, errors);
            hasxml = true;
            xmlErrorText = errors.ToString();
        }

        Context.Source = nav;

        // Reset shared variables
        haserrors = false;
        errors.Clear();

        foreach (var sch in schematrons)
        {
            PerformValidation(sch);
            if (Context.HasErrors)
            {
                haserrors = true;
                errors.Append(Context.Messages.ToString());
            }
        }

        if (haserrors)
        {
            Context.Formatter.Format(schematrons, errors);
            hassch = true;
            schErrorText = errors.ToString();
        }

        errors.Clear();
        if (hasxml)
            errors.Append(xmlErrorText);
        if (hassch)
            errors.Append(schErrorText);

        if (hasxml || hassch)
        {
            Context.Formatter.Format(errors);
            throw new ValidationException(errors.ToString());
        }

        return navdoc;
    }

    void PerformValidation(Schema schema)
    {
        Context.Schema = schema;
        Context.Start();
    }

    void OnValidation(object sender, ValidationEventArgs e)
    {
        haserrors = true;
        Context.Formatter.Format(e, errors);
    }
}

