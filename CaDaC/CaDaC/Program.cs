using System;
using System.Collections.Generic;
using System.Linq;

using System.Notations;
using System.Notations.SExpressions;
using System.Notations.SExpressions.Extensions;

using static System.Notations.Extensions.AnonymousType;

namespace CaDaC // Code As Data / Data As Code
{
    public class PicoNode : SExpression<PicoNode, string>
    {
        public PicoNode(string value) : base(value) { } // for text nodes
        public PicoNode(IEnumerable<PicoNode> content) : base(content) { } // for elements
    }

    public class PicoDocument : PicoNode
    {
        public PicoDocument(PicoElement documentElement) : base(new[] { documentElement }) { }
        public override string ToString() =>
            $"({DocumentElement})";
        public PicoElement DocumentElement => (PicoElement)this[0];
    }

    public class PicoElement : PicoNode
    {
        public PicoElement(string tagName, IEnumerable<PicoNode> content) : base(content) =>
            TagName = tagName;
        public override string ToString() =>
            Count > 0 ? $"<{TagName}>{string.Join(string.Empty, this)}</{TagName}>" : $"<{TagName}/>";
        public string TagName { get; private set; }
    }

    public class PicoText : PicoNode
    {
        public PicoText(string value) : base(value) { }
        public override string ToString() =>
            Value.Substring(1, Value.Length - 2);
    }

    public class FastLexer : SExpressionLexer
    {
        private static readonly ITokenType Identifier = new TokenType("identifier", token => token.Match != "null" ? token.Match : null);
        private static readonly ITokenType Text = new TokenType("text", token => token.Match);
        private static readonly ITokenType Number = new TokenType("number", token => int.Parse(token.Match));
        protected override ITokenType Match(string input, ITokenType tokenType, out string match)
        {
            var at = Offset;
            var ch = input[at];
            var from = at;
            switch (ch)
            {
                case '(':
                case ')':
                    match = new string(new[] { ch });
                    return ch == '(' ? Opening : Closing;
                default:
                    if (char.IsWhiteSpace(ch))
                    {
                        while ((++at < input.Length) && char.IsWhiteSpace(input[at])) ;
                        match = input.Substring(from, at - from);
                        return WhiteSpace;
                    }
                    else if ((ch == '_') || (('A' <= ch) && (ch <= 'Z')) || (('a' <= ch) && (ch <= 'z')))
                    {
                        while
                        (
                            (++at < input.Length) &&
                            (
                                ((ch = input[at]) == '_') ||
                                (('A' <= ch) && (ch <= 'Z')) ||
                                (('a' <= ch) && (ch <= 'z')) ||
                                (('0' <= ch) && (ch <= '9'))
                            )
                        ) ;
                        match = input.Substring(from, at - from);
                        return Identifier;
                    }
                    else if (ch == '"')
                    {
                        ch = (char)0;
                        while ((++at < input.Length) && ((ch = input[at]) != '"')) ;
                        match = ch == '"' ? input.Substring(from, at - from + 1) : string.Empty;
                        return match.Length > 0 ? Text : null;
                    }
                    else if (('0' <= ch) && (ch <= '9'))
                    {
                        while
                        (
                            (++at < input.Length) &&
                            ('0' <= (ch = input[at])) && (ch <= '9')
                        ) ;
                        match = input.Substring(from, at - from);
                        return Number;
                    }
                    else
                    {
                        match = new string(new[] { ch });
                        return Unexpected;
                    }
            }
        }
        public FastLexer() : base(new[] { Identifier, Text, Number }) { }
    }

    public class MyVisitor : System.Linq.Expressions.ExpressionVisitor
    {
        private readonly System.Notations.Runtime.DoubleDispatchObject dispatch;

        public MyVisitor() =>
            dispatch = new System.Notations.Runtime.DoubleDispatchObject(this);
    }

    class Program
    {
        //TODO: CHANGE ME:
        private const string TEST_SEXPR_FILE_PATH = @"C:\Users\User\Desktop\CaDaC\CaDaC\CaDaC\small-sexpr.txt";

        static void Main(string[] arguments)
        {
            var v = new MyVisitor();

            var testLexicon =
                new Lexicon
                (
                    new[]
                    {
                        new TokenType("identifier", "[A-Za-z_][A-Za-z_0-9]*", token => token.Match != "null" ? token.Match : null),
                        new TokenType("text", "\"[^\"]*\"", token => token.Match),
                        new TokenType("number", "[0-9]+", token => int.Parse(token.Match))
                    }
                );

            // Anonymous goodness...
            var AnonymousModel = new { Content = default(object) };

            var AnonymousReducer = Reducer(default(object), AnonymousModel);

            var toAnonymousModel =
                NewReducer
                (
                    AnonymousReducer,
                    (context, outer, expression, value) =>
                        expression ?? new { Content = value }
                );

            var aParser = SExpressionParser.Create(testLexicon, AnonymousReducer);
            var aModel = aParser.Parse("  (  a  123 ( \"foo\" bar ( ) c ) ( null ) ) ", toAnonymousModel);

            System.Diagnostics.Debug.Assert(Is(AnonymousModel, aModel));
            System.Diagnostics.Debug.Assert(Is(ListOf(AnonymousModel), As(AnonymousModel, aModel).Content));
            System.Diagnostics.Debug.Assert(As(ListOf(AnonymousModel), As(AnonymousModel, aModel).Content)[0].Content is string);
            System.Diagnostics.Debug.Assert(As(ListOf(AnonymousModel), As(AnonymousModel, aModel).Content)[0].Content as string == "a");
            System.Diagnostics.Debug.Assert(As(ListOf(AnonymousModel), As(AnonymousModel, aModel).Content)[1].Content is int);
            System.Diagnostics.Debug.Assert((int)As(ListOf(AnonymousModel), As(AnonymousModel, aModel).Content)[1].Content == 123);

            Console.WriteLine();
            Console.Write("Press a key>");
            Console.ReadKey();

            // via extension method : SExpressionBuilder SExpressionBuilder(this object anchor)
            var S = default(object).SExpressionBuilder(); // loosely typed use cases (ie, object / object[] S-Exprs)

            // via extension method : SExpressionBuilder<TExpression> SExpressionBuilder<TExpression>(this TExpression anchor)
            var P = default(PicoNode).SExpressionBuilder(); // strongly typed use cases (ie, PicoNode S-Exprs)

            // equiv S-Expr: ( 123 abc ( x y null z ( null ) ) 456 ( ) )
            // fluently built
            var test1 = S(123, "abc", S("x", "y", null, "z", S(null)), 456, S());
            System.Diagnostics.Debug.Assert(test1 is object[]);
            System.Diagnostics.Debug.Assert((int)(test1 as object[])[0] == 123); // ( 123 <-
            System.Diagnostics.Debug.Assert((string)(test1 as object[])[1] == "abc"); // ( ... abc <-
            System.Diagnostics.Debug.Assert((string)((test1 as object[])[2] as object[])[1] == "y"); // ( ... ( ... y <-
            System.Diagnostics.Debug.Assert((int)(test1 as object[])[3] == 456); // ( ... ( ... ) 456 <-
            System.Diagnostics.Debug.Assert(((object[])(test1 as object[])[4]).Length == 0); // ( ... ( ... ) ... ( ) <-

            // equiv S-Expr: ( ( html ) ( ( body ) ( ( p ) "Hello, world!" ( ( br ) ) "Ciao!" ) ) )
            // fluently built
            var test2 =
                P(
                    P( "html" ),
                    P(
                        P( "body" ),
                        P
                        (
                            P( "p" ),
                            "\"Hello, world!\"",
                            P( P( "br" ) ),
                            "\"Ciao!\""
                        )
                    )
                );
            System.Diagnostics.Debug.Assert(test2 is PicoNode);
            System.Diagnostics.Debug.Assert(test2.IsAtom == false);
            System.Diagnostics.Debug.Assert(test2 is IReadOnlyList<PicoNode>);
            System.Diagnostics.Debug.Assert(test2[0].IsAtom == false);
            System.Diagnostics.Debug.Assert(test2[0][0].IsAtom == true);
            System.Diagnostics.Debug.Assert(test2[0][0].Value == "html");
            System.Diagnostics.Debug.Assert(test2[1][1][1].IsAtom == true);
            System.Diagnostics.Debug.Assert(test2[1][1][1].Value == "\"Hello, world!\"");
            System.Diagnostics.Debug.Assert
            (
                test2.ToString()
                ==
                "( ( System.String{html} ) ( ( System.String{body} ) ( ( System.String{p} ) System.String{\"Hello, world!\"} ( ( System.String{br} ) ) System.String{\"Ciao!\"} ) ) )"
            );
            Console.WriteLine();
            Console.WriteLine($"{nameof(test2)}: {test2}");

            // Now for some parsing...

            var parse1 =
                new SExpressionParser(testLexicon)
                .Parse(" ( 123 abc ( x y null z ( null ) ) 456 ( ) ) ");
            System.Diagnostics.Debug.Assert(parse1 is SExpression);
            System.Diagnostics.Debug.Assert((int)parse1[0].Value == 123); // ( 123 <-
            System.Diagnostics.Debug.Assert((string)parse1[1].Value == "abc"); // ( ... abc <-
            System.Diagnostics.Debug.Assert((string)parse1[2][1].Value == "y"); // ( ... ( ... y <-
            System.Diagnostics.Debug.Assert((string)parse1[2][2].Value == null); // ( ... ( ... null <-
            System.Diagnostics.Debug.Assert((int)parse1[3].Value == 456); // ( ... ( ... ) 456 <-
            System.Diagnostics.Debug.Assert(parse1[4].Count == 0); // ( ... ( ... ) ... ( ) <-
            System.Diagnostics.Debug.Assert
            (
                parse1.ToString()
                ==
                "( System.Int32{123} System.String{abc} ( System.String{x} System.String{y} System.Object{null} System.String{z} ( System.Object{null} ) ) System.Int32{456} (  ) )"
            );
            Console.WriteLine();
            Console.WriteLine($"{nameof(parse1)}: {parse1}");

            var parse2 =
                new SExpressionParser<PicoNode>(testLexicon)
                .Parse(" ( ( html ) ( ( body ) ( ( p ) \"Hello, world!\" ( ( br ) ) \"Ciao!\" ) ) ) ");
            System.Diagnostics.Debug.Assert(parse2 is PicoNode); // Duh.
            System.Diagnostics.Debug.Assert(parse2.IsAtom == false);
            System.Diagnostics.Debug.Assert(parse2 is IReadOnlyList<PicoNode>);
            System.Diagnostics.Debug.Assert(parse2[0].IsAtom == false);
            System.Diagnostics.Debug.Assert(parse2[0][0].IsAtom == true);
            System.Diagnostics.Debug.Assert(parse2[0][0].Value == "html");
            System.Diagnostics.Debug.Assert(parse2[1][1][1].IsAtom == true);
            System.Diagnostics.Debug.Assert(parse2[1][1][1].Value == "\"Hello, world!\"");
            System.Diagnostics.Debug.Assert(parse2.ToString() == test2.ToString()); // Yup.
            Console.WriteLine();
            Console.WriteLine($"{nameof(parse2)}: {parse2}");

            // Parse errors reporting...

            string error1 = null;
            try
            {
                new SExpressionParser(testLexicon).Parse(@"  ( 123
                    ( 456 )
)
 garbage");
            }
            catch (Exception ex)
            {
                error1 = ex.Message;
            }
            System.Diagnostics.Debug.Assert(error1.StartsWith("syntax error: end of input expected but found identifier garbage at line 4, column 2"));

            string error2 = null;
            try
            {
                new SExpressionParser(testLexicon).Parse(@"  ( 123 abc
 456 ");
            }
            catch (Exception ex)
            {
                error2 = ex.Message;
            }
            System.Diagnostics.Debug.Assert(error2.StartsWith("syntax error: ) expected but found end of input at line 2, column 6"));

            string error3 = null;
            try
            {
                new SExpressionParser(testLexicon).Parse(@"  (
    abc (
          ?!? )
 ) ");
            }
            catch (Exception ex)
            {
                error3 = ex.Message;
            }
            System.Diagnostics.Debug.Assert(error3.StartsWith("syntax error: ) expected but found unexpected ?!? at line 3, column 11"));

            // And last but not least...

            // via extension method:
            // Reducer<TContext, TExpression> SExpressionReducer<TContext, TExpression>(this TExpression anchor, TContext prototype, Reducer<TContext, TExpression> reducer)
            var reducer =
                default(PicoNode) // expression type
                .SExpressionReducer
                (
                    default(object), // optional context (ignored)
                    (ignored, outer, expression, value) =>
                        (
                            expression != null ?
                            (
                                !expression.IsAtom &&
                                (expression.Count > 0) &&
                                !expression[0].IsAtom &&    // all this to detect the "(tagName)" in "... ( (tagName) ... ) ..."
                                expression[0].Count == 1 &&
                                expression[0][0].IsAtom ?
                                (
                                    outer == null ?
                                    new PicoDocument(new PicoElement(expression[0][0].Value, expression.Skip(1)))
                                    :
                                    (PicoNode)new PicoElement(expression[0][0].Value, expression.Skip(1))
                                )
                                :
                                (
                                    expression.IsAtom &&
                                    expression.Value.StartsWith("\"") &&
                                    expression.Value.EndsWith("\"") ?
                                    new PicoText(expression.Value)
                                    :
                                    expression
                                )
                            )
                            :
                            SExpressionFactory.Create<PicoNode>(value)
                        )
                );

            /*
             * compare to:
             *  <html>
             *      <body>
             *          <p>
             *              Hello, world!
             *              <br/>
             *              Ciao!
             *          </p>
             *  </html>
             *  ...
             */
            var parse3 =
                new SExpressionParser<PicoNode>(testLexicon)
                .Parse(@"
            (
                ( html )
                (
                    ( body )
                    (
                        ( p ) ""Hello, world!""
                        ( ( br ) )
                        ""Ciao!""
                    )
                )
            ) ", reducer); // <- note the passing of the reducer
            System.Diagnostics.Debug.Assert(parse3 is PicoDocument);
            var documentElement = ((PicoDocument)parse3).DocumentElement;
            System.Diagnostics.Debug.Assert(documentElement.TagName == "html");
            System.Diagnostics.Debug.Assert(documentElement[0] is PicoElement);
            System.Diagnostics.Debug.Assert((documentElement[0] as PicoElement).TagName == "body");
            System.Diagnostics.Debug.Assert(documentElement[0][0] is PicoElement);
            System.Diagnostics.Debug.Assert((documentElement[0][0] as PicoElement).TagName == "p");
            System.Diagnostics.Debug.Assert(documentElement[0][0][0] is PicoText);
            System.Diagnostics.Debug.Assert((documentElement[0][0][0] as PicoText).Value == "\"Hello, world!\"");
            System.Diagnostics.Debug.Assert
            (
                parse3.ToString()
                ==
                "(<html><body><p>Hello, world!<br/>Ciao!</p></body></html>)"
            );
            Console.WriteLine();
            Console.WriteLine($"{nameof(parse3)}: {parse3}");

            //var input = System.IO.File.ReadAllText(TEST_SEXPR_FILE_PATH);
            //var parser = new SExpressionParser<PicoNode>();

            //Console.WriteLine();
            //Console.Write("Press a key>");
            //Console.ReadKey();

            //var sw = new System.Diagnostics.Stopwatch();
            //sw.Start();
            //var parse4 = parser.Parse(input, new FastLexer(), reducer);
            //sw.Stop();
            //Console.WriteLine();
            //Console.WriteLine($"{nameof(parse4)}: elapsed: {sw.ElapsedMilliseconds.ToString("0,0")} ms.");

            //Console.WriteLine();
            //Console.Write("Press a key>");
            //Console.ReadKey();

            //var body = (PicoElement)((PicoDocument)parse4).DocumentElement[0];
            //var firstParagraph = (PicoElement)body[0];
            //Console.WriteLine();
            //Console.WriteLine($"{nameof(parse4)}: '{body.TagName}' has {body.Count} child nodes '{firstParagraph.TagName}'");

            //Console.WriteLine();
            //Console.Write("Press a key>");
            //Console.ReadKey();

            //Console.WriteLine();
            //Console.WriteLine($"{nameof(parse4)}: {parse4}");

            Console.WriteLine();
            Console.Write("The end>");
            Console.ReadKey();
        }
    }
}