using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Collections;


namespace GeneticAlgorithm
{
    enum Type
    {
        parent = 1,
        child = 2,
        mutant = 3
    }
    class Individual : GeneticAlgorithm, IComparable<Individual>
    {

        public int index;
        double a = 6;

        public Type Type;
        int[] chromo_1;


        public int[] chromosome_1
        {
            get
            {
                return chromo_1;
            }
            set
            {
                chromo_1 = value;
            }
        }

        double funcVal;
        public double funcValue
        {
            get
            {
                return funcVal;
            }
            private set
            {
                funcVal = value;
            }
        }

        public int chromoDecimalValue_1;


        public Individual(int index, int[] chromo_1, Type type)
        {
            this.index = index;
            this.Type = type;
            this.chromo_1 = chromo_1;
        }
        public int CompareTo(Individual p)
        {
            return this.funcVal.CompareTo(p.funcVal);
        }

        private int TurnToDecimal(int[] chromo_asArray)
        {

            int L = 9;
            string s = "";
            for (int i = 0; i < L; i++)
            {
                s += Convert.ToString(chromo_asArray[i]);
            }
            /*
            StringBuilder chromo_asStringBuilder = new StringBuilder();

            for (int i = 0; i < L; i++)
                chromo_asStringBuilder.Append(chromo_asArray[i]);
           */
            int result = Convert.ToInt32(s);
            return result;
        }

        public double MaxValue(int[,] arr)
        {
            int Max = -10000;
            for (int i = 0; i < arr.GetLength(0); i++)
                for (int j = 0; j < arr.GetLength(1); j++)
                    if (arr[i, j] > Max)
                        Max = arr[i, j];
            return Max;
        }



        private double CalculateFunctionValue(int[] chromosome)
        {
            string[] lines = File.ReadAllLines(@"NumAntenn.txt");
            int[,] NumAntenn = new int[lines.Length, lines[0].Split(' ').Length];
            for (int i = 0; i < lines.Length; i++)
            {
                string[] temp = lines[i].Split(' ');
                for (int j = 0; j < temp.Length; j++)
                    NumAntenn[i, j] = Convert.ToInt32(temp[j]);
            }
            lines = File.ReadAllLines(@"matrix.txt");
            int[,] Matrix = new int[lines.Length, lines[0].Split(' ').Length];
            for (int i = 0; i < lines.Length; i++)
            {
                string[] temp = lines[i].Split(' ');
                for (int j = 0; j < temp.Length; j++)
                    Matrix[i, j] = Convert.ToInt32(temp[j]);
            }

            List<int> PRD = new List<int>();
            List<int> PRM = new List<int>();
            PRD.Add(0); PRD.Add(1); PRD.Add(2);
            PRM.Add(3); PRM.Add(4); PRM.Add(5); PRM.Add(6);

            /*
            for (int i = 0; i < NumAntenn.GetLength(1); i++)
            {
                if (NumAntenn[i, 2] == 1)
                    PRD.Add(NumAntenn[i, 0] - 1);
                else
                    PRM.Add(NumAntenn[i, 0] - 1);
            }*/

            int[,] result = new int[chromosome.Length, chromosome.Length];

            for (int i = 0; i < PRD.Count; i++)
            {
                for (int j = 0; j < PRM.Count; j++)
                {
                    result[i, j] = Matrix[chromosome[i], chromosome[j]] - NumAntenn[PRM[j], 1];
                }
            }

            return MaxValue(result);
        }

        public void GetSolution()
        {
            chromoDecimalValue_1 = TurnToDecimal(chromo_1);
            funcValue = CalculateFunctionValue(chromo_1);
        }

    }
}
