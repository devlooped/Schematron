using System;

namespace Schematron
{
	/// <summary>
	/// A Rule element.
	/// </summary>
	/// <remarks>
	/// Rules enclose the <see cref="Assert"/> and <see cref="Report"/>
	/// elements, providing the context for their evaluation.
	/// According to the <link ref="schematron" />, nodes can be evaluated
	/// by only one rule inside a <see cref="Pattern"/>, so typical schema
	/// design includes placing the most exceptional rules first, down to
	/// more generally applicable ones.
	/// <para>
	/// Constructor is not public. To programatically create an instance of this
	/// class use the <see cref="Pattern.CreateRule(string)"/> factory method.
	/// </para>
	/// </remarks>
	/// <author ref="dcazzulino">
	/// Should we add support for the key element? <link ref="schematron" />
	/// </author>
	/// <progress amount="100" />
	public class Rule : EvaluableExpression
	{
		// TODO: add support to <key></key> child elements?

		TestCollection _asserts = new TestCollection();
		TestCollection _reports = new TestCollection();
		string _id = String.Empty;
		bool _abstract = true;

		/// <summary>
		/// Creates an abstract rule, without context.
		/// </summary>
		internal protected Rule()
		{
		}

		/// <summary>Initializes a new instance of the class, with the received context.</summary>
		/// <param name="context">The rule's context to evaluate.</param>
		/// <remarks>If passed a null or an <see cref="String.Empty"/>, this is implicitly an abstract rule.</remarks>
		internal protected Rule(string context)
		{
			InitContext(context);
		}

		/// <summary>Initializes the context for the rule.</summary>
		/// <param name="context">The rule's context to evaluate.</param>
		/// <remarks>
		/// If passed a null or an <see cref="String.Empty"/>, this is implicitly an abstract rule.
		/// </remarks>
		/// <author ref="dcazzulino">
		/// Rules are evaluated through all the document (//), unless they
		/// explicitly want to start from the root (/). This is consistent
		/// with XSLT template match behavior. So we have to split the expression
		/// per union (|) to add the root expression in these cases.
		/// </author>
		private void InitContext(string context)
		{
			if (context == null || context == String.Empty)
			{
				_abstract = true;
				return;
			}


			// Rules are evaluated through all the document (//), unless they
			// explicitly want to start from the root (/).
			// We have to split per union (|) to add the root expression.
			string[] parts = context.Split('|');

			for (int i = 0; i < parts.Length; i++)
			{
				// Trim to build the string properly.
				parts[i] = parts[i].Trim();

				// Append the slashes as appropriate.
				if (!parts[i].StartsWith("/"))
					parts[i] = "//" + parts[i];
			}

			// Finally, join again the location paths and initialize the expression.
			InitializeExpression(String.Join(" | ", parts));
			_abstract = false;
		}

		#region Overridable Factory Methods
		/// <summary>Creates a new assert instance.</summary>
		/// <remarks>
		/// Inheritors should override this method to create instances
		/// of their own assert implementations.
		/// </remarks>
		/// <param name="test">
		/// The XPath expression to test for the assert. See
		/// <see cref="EvaluableExpression.Expression"/>.
		/// </param>
		/// <param name="message">
		/// The message to display if the assert fails. See
		/// <see cref="Test.Message"/>.
		/// </param>
		public virtual Assert CreateAssert(string test, string message)
		{
			return new Assert(test, message);
		}

		/// <summary>Creates a new report instance.</summary>
		/// <remarks>
		/// Inheritors should override this method to create instances
		/// of their own report implementations.
		/// </remarks>
		/// <param name="test">
		/// The XPath expression to test for the report. See
		/// <see cref="EvaluableExpression.Expression"/>.
		/// </param>
		/// <param name="message">
		/// The message to display if the report succeeds. See
		/// <see cref="Test.Message"/>.
		/// </param>
		public virtual Report CreateReport(string test, string message)
		{
			return new Report(test, message);
		}
		#endregion

		/// <summary />
		public string Id
		{
			get { return _id; }
			set { _id = value; }
		}

		/// <summary />
		public string Context
		{
			get { return base.Expression; }
			set { InitContext(value); }
		}

		/// <summary />
		public bool IsAbstract
		{
			get { return (_abstract); }
		}

		/// <summary />
		public TestCollection Asserts
		{
			get { return _asserts; }
		}

		/// <summary />
		public TestCollection Reports
		{
			get { return _reports; }
		}

		/// <summary />
		public void AddAssert(string test, string message)
		{
			_asserts.Add(CreateAssert(test, message));
		}

		/// <summary />
		public void AddReport(string test, string message)
		{
			_reports.Add(CreateReport(test, message));
		}

		/// <summary />
		/// <param name="parent"></param>
		/// <exception cref="InvalidOperationException">Only abstract rules can be used as a base for extensions.</exception>
		public void Extend(Rule parent)
		{
			if (!parent.IsAbstract)
				throw new ArgumentException("The rule to extend must be abstract.", "parent");

			foreach (Assert asr in parent._asserts)
				_asserts.Add(asr);

			foreach (Report rpt in parent._reports)
				_reports.Add(rpt);
		}
	}
}
