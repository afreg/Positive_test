
namespace IfElse_JavaGenerator
{
    class Program
    {
        static void Main(string filename, int maxConds, int maxDepth)
        //static void Main()
        {
            //string filename = "C:\\Users\\NatAn\\source\\repos\\Positive_Tech_test\\Example.java";
            FileInfo fileInf = new FileInfo(filename);
            if (fileInf.Exists) fileInf.Delete();
            Random rnd = new Random();

            //int maxConds = 8;
            //int maxDepth = 6;
            
            using (StreamWriter sw = File.AppendText(filename))
            {
                HeadCreator(filename, sw);
                BlockCreator(maxConds, 0, maxDepth, sw, rnd);
                TailCreator(sw);
            }
        }

        private static void HeadCreator(string path, StreamWriter sw)
        {
            sw.WriteLine("public class Main {");
            sw.WriteLine("public static void method(boolean... conditions) {");
            sw.WriteLine("int x;");
        }

        private static void TailCreator(StreamWriter sw)
        {
            sw.WriteLine("System.out.println(x);");
            sw.WriteLine("}");
            sw.WriteLine("}");
        }

        private static int BlockCreator(int maxConds, int xValue, int remDepth, StreamWriter sw, Random rnd)
        {
            var x = xValue;
            int nStatements = rnd.Next(remDepth-2<0?0:remDepth - 2, remDepth);
            Console.WriteLine("Block of " + nStatements + " statements (depth)" + remDepth);
            for (int it = 0; it < nStatements; ++it)
            {
                if (rnd.NextDouble() >= 0.3)
                    x = BranchCreator(maxConds, x, remDepth, sw, rnd);
                else
                {
                    sw.WriteLine("x = " + x +";");
                    x++;
                }
            }
            return x;
        }
        
        private static int BranchCreator(int maxConds, int xValue, int remDepth, StreamWriter sw, Random rnd)
        {
            var x = xValue;
            sw.WriteLine("if (conditions[" + rnd.Next(0,maxConds) + "]) {");
            x = BlockCreator(maxConds, x, remDepth-1, sw, rnd);
            sw.WriteLine("}");
            if (rnd.NextDouble() >= 0.5)
            {
                sw.WriteLine("else {");
                x = BlockCreator(maxConds, xValue, remDepth-1, sw, rnd);
                sw.WriteLine("}");
            }
            return x;
        }
    }
}