
## XPath Execution:
- Every XPath execution involves:
- Creating an XPathNavigator (if it’s not executed directly against one)
- Compiling the expression, which in turn involves:
- Building an IQuery which consists of:
- Parsing the expression: XPathParser.ParseXPathPattern (using a parser and a scanner)
- Processing the AstNode built in the previous step: nodes, axis, operators, etc. Objects for each of them.
- Initializing an CompiledXPathExpr object from the previous ones.
- Constructing an XPathSelectionIterator with the expression, which involves:
- Cloning the source navigator
- Retrieving the query from the compiled expression
- Setting the query context to another clone of the source navigator
- On iteration through the list, the query is advanced until it reaches the end. Each advance returns a new XPathNavigator node which is used afterwards (or null if nothing else is found). 
- Except if it’s executed through an Evaluate and the ReturnType is not a ReturnType.NodeSet, in which case, the last 2 steps are ommited, and the expression is directly evaluated to an object (the IQuery built in this case can be an AndExpr, LogicalExpr, NumberFunctions, NumericExpr, etc).


## Other:
- The XPathNavigator.Clone method only creates a new object and saves the references to the document, the node and parentOfNs (?) variables. 
- XPathExpression objects which contain non XPath 1.0 functions, such as current(), generate-id(), key(), format-number(), etc., must have an XsltContext at the XsltFunction level (a type of IQuery built previously after expression parsing). The problem here is that XsltFunction is a private class and its SetXsltContext() method is internal. So there’s no way to explicitly pass this context. We will provide a fail-over mechanism that will begin processing the schema using the tradicional XSLT approach. This may significantly reduce the performance (MEASSURE!!) and memory usage (MEASURE!!)
- Transformations take an IXPathNavigable (an XPathDocument or XmlNode). We will use XPathDocument because it can build an XPathNavigator we will need to compile and evaluate XPathExpressions. (old... for transformation implementation only)
- The only way to get at the xml contents of a navigator is to check whether it implements IHasXmlNode, which is only true if the navigator was constructed from a loded XmlDocument (ugh...)
- When we use an XPathNodeIterator, the iterator.Current object is always the same, that is, a single object is created, and its internal values changed to reflect the undelying current node. That’s why we can’t use the object’s hashcode or reference to uniquely distinguish already matched nodes. The only way is to use IsSamePosition method, which forces us to iterate through a collection of previously saved navigators. Note that we must clone the Current element, or the position will be changed as we move on.
- XPathNavigator.Evaluate() produces a movement in the cursor position. Always remember to clone before doing anything against a navigator, or clone once, and later use MoveTo(XPathNavigator) to reposition again to the original place.


## Performance-related:
- From MSDN (): “Innovative cursor style navigation of the XPathNavigator, which minimizes node creation to a single virtual node, yet provides random access to the document. This does not require a complete node tree to be built in memory like the DOM.”. So, test speed by doing XSD validation on one step and XPathDocument schematron validation in another step.
- For all but the smallest documents (or very few child nodes), XPathNavigator.SelectChildren and XPathNavigator.SelectDescendents is 35-45 slower than XPathNavigator.Select with a precompiled expression. 
- Adding the string values that are expected in the schema to the navigator’s NameTable property, prior to executing the queries, offers a marginal performance increase of 5-10%.  

