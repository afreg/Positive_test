using Antlr4.Runtime;

namespace Parse_IfElse_result
{
    // List of possibilities
    using PossibList = LinkedList<KeyValuePair<SortedSet<String>, int>>;

    class Program
    {
        static void Main(string filename)
        //static void Main()
        {
            try
            {
                //var filename = "C:\\Users\\NatAn\\source\\repos\\Positive_Tech_test\\Example.java"; // args[0] ?
                var fileContents = File.ReadAllText(filename);

                var input = new AntlrInputStream(fileContents);
                var lexer = new JavaLexer(input);

                var tokens = new CommonTokenStream(lexer);
                var parser = new JavaParser(tokens);

                var context = parser.methodDeclaration();

                var visitor = new BranchResults_JavaVisitor();
                var possibilities = visitor.Visit(context);

                //PossibOut(possibilities);
                Out(possibilities);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
        }

        private static void PossibOut(PossibList p_list)
        {
            // Console output of possible logic combinations with corresponding result
            foreach (var possib in p_list)
            {
                Console.Write("Conditions:\t{ ");
                foreach (var log in possib.Key) Console.Write(log + " ");
                Console.Write("}\nValue:\t\t[ possib.Value ]\n");
            }
        }

        private static void Out(PossibList p_list)
        {
            // Console output of possible results set
            var results = new SortedSet<int>();
            foreach (var possib in p_list)
                    results.Add(possib.Value);
            var res_str = new String("[");
            foreach (int res in results)
            {
                res_str += res.ToString();
                if (res != results.Max)
                    res_str += ", ";
            }
            res_str += "]";
            Console.WriteLine(res_str);
        }

    }
}

