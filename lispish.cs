/*
<Program> ::= {<SExpr>}
<SExpr> ::= <Atom> | <List>
<List> ::= () | ( <Seq> )
<Seq> ::= <SExpr> <Seq> | <SExpr>
<Atom> ::= ID | INT | REAL | STRING


LITERAL = [\(\)]
REAL = [+-]?[0-9]*\.[0-9]+
INT = [+-]?[0-9]+
STRING = "(?>\\.|[^\\"])*".  Multiline strings are not allowed.
ID = [^\s"\(\)]+
Anything else other than whitespace is an error ( INVALID)
*/
using System;
using System.Collections.Generic;
using  System.Text.RegularExpressions;

public class LispishParser {
     public  class Parser {
        List<Node> tokens = new List<Node>();
        int cur = 0;
        
        public Parser(Node [] tokens) {
            this.tokens = new List<Node>(tokens);
            this.tokens.Add(new Node(Symbols.INVALID, ""));
        }

        public Node ParseProgram() {
            var children = new List<Node>();

            while (tokens[cur].Symbol != Symbols.INVALID){
                children.Add(ParseSExpr());
            }
            
            return new Node(Symbols.Program, children.ToArray());
        }

        public Node ParseSExpr() {
            // <SExpr> ::= <Atom> | <List>
            var children = new List<Node>();

            if (tokens[cur].Text == "(") {
                return new Node(Symbols.SExpr, ParseList());
            }
            else {
                return new Node(Symbols.SExpr, ParseAtom());
            }

        }

        public Node ParseList() {
            /////////////////////////////
            // <List> ::= () | ( <Seq> )
            var children = new List<Node>();


            var lparen = ParseLiteral("(");
            children.Add(lparen);
            if (tokens[cur].Text == ")") {
                return new Node(Symbols.List, lparen, ParseLiteral(")"));
            }
            else {
                while (tokens[cur].Text != ")") {
                    children.Add(ParseSeq());
                }
                var rparen = ParseLiteral(")");
                children.Add(rparen);
                return new Node(Symbols.List, children.ToArray());
            }
        }

        public Node ParseSeq() {
            // <Seq> ::= <SExpr> <Seq> | <SExpr>
            var children = new List<Node>();
            while (tokens[cur].Text != ")") {
                children.Add(ParseSExpr());
                if (tokens[cur].Text != ")") {
                    children.Add(ParseSeq());
                }
                
            }
            return new Node(Symbols.Seq, children.ToArray());
        }

        public Node ParseAtom() {
            // <Atom> ::= ID | INT | REAL | STRING
            return new Node(Symbols.Atom,  tokens[cur++]);
        }

        public Node ParseLiteral(string lit) {
            if (tokens[cur].Text == lit){
                return tokens[cur++];
            } else {
                throw new Exception("Syntax error");
            }
        }
    }
    public enum Symbols {
        LITERAL,
        REAL,
        INT,
        STRING,
        ID,
        INVALID,

        Program,
        SExpr,
        List,
        Seq,
        Atom
    }
    public class Node {
        public Symbols Symbol;
        public string Text = "";

        List<Node> children = new List<Node>();

        public Node(Symbols symbol, string text){
            this.Symbol = symbol;
            this.Text = text;
        }
        public Node(Symbols symbol, params Node[] children){
            this.Symbol = symbol;
            this.Text = "";
            this.children = new List<Node>(children);
        }
        public void Print(string prefix = "")
        {
            Console.WriteLine($"{prefix}{Symbol.ToString().PadRight(42-prefix.Length)} {Text}");
            foreach (var child in children){
                child.Print(prefix+"  ");
            }
        }
    }

    static public List<Node> Tokenize(String src)
    {
        var result = new List<Node>();
        int pos = 0;
        Match m;

        /*
        LITERAL = [\(\)]
        REAL = [+-]?[0-9]*\.[0-9]+
        INT = [+-]?[0-9]+
        STRING = "(?>\\.|[^\\"])*".  Multiline strings are not allowed.
        ID = [^\s"\(\)]+
        Anything else other than whitespace is an error ( INVALID)
        */

        var WS = new Regex(@"\G\s");
        var LITERAL = new Regex(@"\G[\(\)]");
        var REAL = new Regex(@"\G[+-]?[0-9]*\.[0-9]+");
        var INT = new Regex(@"\G[+-]?[0-9]+");
        var STRING = new Regex(@"\G""(?>\\.|[^\\""])*""");
        var ID = new Regex(@"\G[^\s""\(\)]+");

        while (pos < src.Length) {
            if ((m = WS.Match(src, pos)).Success) {
                pos += m.Length;
            } else if ((m = LITERAL.Match(src, pos)).Success){
                result.Add(new Node(Symbols.LITERAL, m.Value));
                pos += m.Length;
            } else if ((m = REAL.Match(src, pos)).Success){
                result.Add(new Node(Symbols.REAL, m.Value));
                pos += m.Length;
            } else if ((m = INT.Match(src, pos)).Success){
                result.Add(new Node(Symbols.INT, m.Value));
                pos += m.Length;
            } else if ((m = STRING.Match(src, pos)).Success){
                result.Add(new Node(Symbols.STRING, m.Value));
                pos += m.Length;
            } else if ((m = ID.Match(src, pos)).Success){
                result.Add(new Node(Symbols.ID, m.Value));
                pos += m.Length;
            } else {
                throw new Exception("Lexer error");
            }
        }
        return result;
    }

    static public Node Parse(Node[] tokens)
    {
        var p = new Parser(tokens);
        var tree = p.ParseProgram();
        return tree;
    }

    static private void CheckString(string lispcode)
    {
        try
        {
            Console.WriteLine(new String('=', 50));
            Console.Write("Input: ");
            Console.WriteLine(lispcode);
            Console.WriteLine(new String('-', 50));

            Node[] tokens = Tokenize(lispcode).ToArray();

            Console.WriteLine("Tokens");
            Console.WriteLine(new String('-', 50));
            foreach (Node node in tokens)
            {
                Console.WriteLine($"{node.Symbol,-21}\t: {node.Text}");
            }
            Console.WriteLine(new String('-', 50));

            Node parseTree = Parse(tokens);

            Console.WriteLine("Parse Tree");
            Console.WriteLine(new String('-', 50));
            parseTree.Print();
            Console.WriteLine(new String('-', 50));
        }
        catch (Exception)
        {
            Console.WriteLine("Threw an exception on invalid input.");
        }
    }


    public static void Main(string[] args)
    {
        //Here are some strings to test on in 
        //your debugger. You should comment 
        //them out before submitting!

        // CheckString(@"(define foo 3)");
        // CheckString(@"(define foo ""bananas"")");
        // CheckString(@"(define foo ""Say \\""Chease!\\"" "")");
        // CheckString(@"(define foo ""Say \\""Chease!\\)");
        // CheckString(@"(+ 3 4)");      
        // CheckString(@"(+ 3.14 (* 4 7))");
        // CheckString(@"(+ 3.14 (* 4 7)");

        CheckString(Console.In.ReadToEnd());
    }
}