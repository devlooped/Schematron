using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;
using System.Xml.XPath;
using Xunit;

namespace Schematron.Tests;

public class ValidatorTests
{
    const string XsdLocation = "./Content/po-schema.xsd";
    const string XsdWithPartialSchemaLocation = "./Content/po-schema-with-schema-import.xsd";
    const string XmlContentLocation = "./Content/po-instance.xml";
    const string TargetNamespace = "http://example.com/po-schematron";

    [Fact]
    public void NewAddSchemaSignatureShouldNotBreakCode()
    {
        var validatorA = new Validator(OutputFormatting.XML);
        validatorA.AddSchema(XmlReader.Create(XsdLocation));

        var validatorB = new Validator(OutputFormatting.XML);
        validatorB.AddSchema(TargetNamespace, XsdLocation);

        string? resultA = default;
        string? resultB = default;

        try
        {
            var result = validatorA.Validate(XmlReader.Create(XmlContentLocation));
        }
        catch (ValidationException ex)
        {
            resultA = ex.Message;

            System.Diagnostics.Debug.WriteLine(ex.Message);
        }

        try
        {
            var result = validatorB.Validate(XmlReader.Create(XmlContentLocation));
        }
        catch (ValidationException ex)
        {
            resultB = ex.Message;

            Xunit.Assert.True(resultA == resultB);

            System.Diagnostics.Debug.WriteLine(ex.Message);
        }

    }

    [Fact]
    public void ValidateShouldReturnSchematronValidationResultWhenSchematronConstraintsAreNotMet()
    {
        //Arrange
        var validator = new Validator(OutputFormatting.XML);

        //Act
        validator.AddSchema(TargetNamespace, XsdLocation);

        using (var doc = XmlReader.Create(XmlContentLocation))
        {
            IXPathNavigable? result = default;

            try
            {
                result = validator.Validate(doc);
            }
            catch (ValidationException ex)
            {
                System.Diagnostics.Debug.WriteLine(ex.Message);

                var serializer = new XmlSerializer(typeof(Schematron.Serialization.SchematronValidationResultTempObjectModel.Output));

                using (var stream = new MemoryStream(System.Text.Encoding.Unicode.GetBytes(ex.Message)))
                using (var reader = XmlReader.Create(stream))
                {
                    var obj = (Schematron.Serialization.SchematronValidationResultTempObjectModel.Output)serializer.Deserialize(reader);

                    // Assert


                    Xunit.Assert.NotNull(obj);
                    Xunit.Assert.NotNull(obj.Xml);
                    Xunit.Assert.NotNull(obj.Schematron);
                }
            }
        }
    }

    [Fact]
    public void WhenUsingTheXmlReaderApproach_ToSupplyASchema_TypesFromImportsAreNotResolved()
    {
        // arrange
        var validator = new Schematron.Validator();

        // act, (assert)
        Xunit.Assert.Throws<XmlSchemaException>(() => validator.AddSchema(XmlReader.Create(XsdWithPartialSchemaLocation)));
    }

    [Fact]
    public void WhenUsingTheXmlSchemaSetBasedApproach_ToSupplyASchema_TypesFromImportsAreResolved()
    {
        // arrange
        var validator = new Schematron.Validator();

        var count = validator.XmlSchemas != null ? validator.XmlSchemas.Count : 0;

        // act, (assert)
        validator.AddSchema(TargetNamespace, XsdWithPartialSchemaLocation);

        Xunit.Assert.True(validator.Schemas.Count == count + 1);

        //var res = validator.Validate(XmlContentLocation);
    }

    //[Fact]
    public void DoTheRawXmlValidation()
    {
        throw new NotImplementedException();
    }

    [Fact]
    public void SchematronValidationResultIncludesExpandedValueElements()
    {
        //Arrange
        var validator = new Validator(OutputFormatting.XML);

        //Act
        validator.AddSchema(TargetNamespace, XsdLocation);

        using (var doc = XmlReader.Create(XmlContentLocation))
        {
            var result = (IXPathNavigable)null;

            try
            {
                result = validator.Validate(doc);
            }
            catch (ValidationException ex)
            {
                System.Diagnostics.Debug.WriteLine(ex.Message);
                string expectedMessage = "<text>Attributes sex (Female) and title (Mr) must have compatible values on element customer.</text>";
                Xunit.Assert.True(ex.Message.Contains(expectedMessage));
            }
            Xunit.Assert.Null(result);
        }
    }

}

