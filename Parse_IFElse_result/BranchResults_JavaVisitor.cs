using Antlr4.Runtime;
using Antlr4.Runtime.Misc;

namespace Parse_IfElse_result
{
    // List of IF conditions with its bool value attached (+ for IF and - for ELSE)
    using LogicCond = SortedSet<String>;
    // Pair of Logic condition and possible x values
    using Possibility = KeyValuePair<SortedSet<String>, int>;
    // List of possibilities
    using PossibList = LinkedList<KeyValuePair<SortedSet<String>, int>>;

    public class BranchResults_JavaVisitor : JavaParserBaseVisitor<PossibList>
    {
        public override PossibList VisitStatement([NotNull] JavaParser.StatementContext context)
        {
            var possibilities = new PossibList();

            if (context.IF() != null)
            {
                var cond = context.parExpression().expression().GetText();
                {
                    //Console.WriteLine("IF statement " + context.parExpression().expression().GetText());
                    var inner = Visit(context.statement(0));
                    possibilities = IfElseMerge(possibilities, inner, '+' + cond);
                }
                if (context.ELSE() != null)
                {
                    //Console.WriteLine("ELSE statement");
                    var inner = Visit(context.statement(1));
                    possibilities = IfElseMerge(possibilities, inner, '-' + cond);
                }
            }
            else if (context.block() != null)
            {
                var inner = Visit(context.block());
                possibilities = Cleanup(possibilities, inner);
            }
            else if (context.statementExpression != null)
            {
                var inner = Visit(context.statementExpression);
                possibilities = Cleanup(possibilities, inner);
            }
            return possibilities;
        }

        private static PossibList IfElseMerge(PossibList old_one, PossibList new_one, String cond)
        {
            // Merge logic conditions of IF operator with its inner block conditions
            // (If A {B}) = A&B
            // (else {C)) = (-A)&C
            var inner = new PossibList();
            String opp_cond = OppositeCondition(cond);
            foreach (var possib in new_one)
            {
                LogicCond log = possib.Key;
                // collect conditions into set
                // A&(B&(-A)) = 0, delete
                if (!log.Contains(opp_cond))
                {
                    log.Add(cond);
                    inner.AddLast(new Possibility(log, possib.Value));
                }
            }
            old_one = Cleanup(old_one, inner);
            return old_one;
        }

        //
        private static String OppositeCondition(String cond)
        {
            // For cond (A) returns cond (-A)
            String temp = "";
            if (cond.StartsWith('+'))
                temp = '-' + cond.Substring(1);
            else if (cond.StartsWith('-'))
                temp = '+' + cond.Substring(1);
            return temp;
        }

        private static PossibList Cleanup(PossibList old_one, PossibList new_one)
        {
            // Merge possibilities lists with cleanup
            if (new_one.Count > 0)
            {
                foreach (var possib in new_one) old_one.AddLast(possib);

                for (var node = old_one.Last; node != null;)
                {
                    for (var node_check = node.Previous; node_check != null;)
                    {
                        var temp = node_check.Previous;
                        // Cond(1) A&B and later cond(2) B means (A&B)vB = B is covered
                        // cond(1) is subset of cond(2) => delete cond(1)
                        if (node.Value.Key.IsSubsetOf(node_check.Value.Key))
                            old_one.Remove(node_check);
                        // Cond(1) A&B and later cond(2) (-B) means (A&B)v(-B) = Av(-B) is covered
                        // Trim cond(1) to A and leave cond(2) as it is
                        else
                            foreach (var cond in node.Value.Key)
                            {
                                LogicCond cond_to_check = new LogicCond(node.Value.Key);
                                cond_to_check.Remove(cond);
                                String n_cond = OppositeCondition(cond);
                                cond_to_check.Add(n_cond);
                                if (cond_to_check.IsSubsetOf(node_check.Value.Key))
                                    node_check.Value.Key.Remove(n_cond);
                            }
                        node_check = temp;
                    }
                    node = node.Previous;
                }
            }
            return old_one;
        }

        public override PossibList VisitBlock([NotNull] JavaParser.BlockContext context)
        {
            var possibilities = new PossibList();

            for (int it = 1; it < context.ChildCount - 1; ++it)
            {
                var inner = Visit(context.GetChild(it));
                possibilities = Cleanup(possibilities, inner);
            }

            return possibilities;
        }

        public override PossibList VisitBlockStatement([NotNull] JavaParser.BlockStatementContext context)
        {
            var possibilities = new PossibList();

            if (context.statement() != null)
                possibilities = Visit(context.statement());

            return possibilities;
        }

        public override PossibList VisitExpression([NotNull] JavaParser.ExpressionContext context)
        {
            var possibilities = new PossibList();

            if (context.bop != null)
            {
                if (context.bop.Text[0] == '=' & context.bop.Text.Length == 1)
                    possibilities = Visit(context.expression(1));
            }
            else if (context.primary() != null)
                possibilities = Visit(context.primary());

            return possibilities;
        }

        public override PossibList VisitPrimary([NotNull] JavaParser.PrimaryContext context)
        {
            var possibilities = new PossibList();

            if (context.literal() != null)
            {
                // Create new LogicCondition for this leaf
                var n_pair = new Possibility(new LogicCond(), int.Parse(context.literal().GetText()) );
                possibilities.AddLast(n_pair);
            }

            return possibilities;
        }
    }
}
