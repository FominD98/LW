using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace GeneticAlgorithm
{	
	class ResearchResult
	{
		public int expNumber;
		public string paramValue;
		public double solutionQuality, convergenceRate;

		public ResearchResult(int expNumber, string paramVal, double solutionQuality, double convergenceRate)
		{
			this.expNumber = expNumber;
			this.paramValue = paramVal;
			this.solutionQuality = solutionQuality;
			this.convergenceRate = convergenceRate;
		}
	}
	class ControlledParameter
	{
		public string name;
		public double minValue, maxValue, currentValue, step;
		public List<ResearchResult> researchResults;

		public ControlledParameter(string name, double minValue, double maxValue, double step)
		{
			this.name = name;
			this.minValue = minValue;
			this.maxValue = maxValue;
			this.currentValue = minValue;
			this.step = step;
			researchResults = new List<ResearchResult>(); 
		}
	}
	enum OperatorsMode
	{
		defaultOperators,
		customOperators
	}

	class GeneticAlgorithm : Program
	{
        double bestFunctionValue = double.NaN;
		int stagesCounter = 0;
		readonly double E = 0.1;
		readonly int neededStagesOfStability = 8;
		readonly int L = 9;
		readonly double functionMin = 0.01;

		OperatorsMode operatorsMode = OperatorsMode.customOperators;
		public bool printGADetails;

		ControlledParameter PopulationSize = new ControlledParameter("Размерность популяции", 10, 100, 10);
		ControlledParameter CrossingChance = new ControlledParameter("Вероятность скрещивания", 0.1f, 1, 0.1f);
		ControlledParameter MutationChance = new ControlledParameter("Вероятность мутации", 0.1f, 1, 0.1f);
		List<ControlledParameter> ControlledParameters = new List<ControlledParameter>();

		List<Individual> population;
		List<Individual> survivors;
		List<Tuple<Individual,Individual>> parentCouples;
       

      
        public void ReadFile()
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
            lines = File.ReadAllLines(@"NumPos.txt");
            int[,] NumPos = new int[lines.Length, lines[0].Split(' ').Length];
            for (int i = 0; i < lines.Length; i++)
            {
                string[] temp = lines[i].Split(' ');
                for (int j = 0; j < temp.Length; j++)
                    NumPos[i, j] = Convert.ToInt32(temp[j]);
            }
        }

        public void SetOperatorsMode(OperatorsMode operatorsMode)
		{
			this.operatorsMode = operatorsMode;
		}

		public void ExlopreEfficiency()
		{
			int expIterations = 10;
			List<ControlledParameter> ControlledParameters = new List<ControlledParameter>() { PopulationSize, CrossingChance, MutationChance };
            
			foreach (var param in ControlledParameters)
			{
				//параметры кроме текущего устанавливаются на максимум
				for (int c = 0; c < ControlledParameters.Count; c++)
					if (ControlledParameters[c] != param)
						ControlledParameters[c].currentValue = ControlledParameters[c].maxValue;

				for (double i = Math.Round(param.minValue,2), counter = 0; i <= param.maxValue; i +=  Math.Round(param.step,2) , counter++)
				{
					param.currentValue = i;
					double sumOfFuncBestValues = 0;
					int sumOfConvRates = 0;

					//Console.WriteLine("\nИсследование: {" + param.name + "} \nЭксперимент №" + counter + "; значение параметра = " + i);
					for (int j = 0; j < expIterations; j++)
					{
						PerformGeneticAlgorithm();
						//Console.WriteLine("		тык" + bestFunctionValue);
						sumOfFuncBestValues += bestFunctionValue;
						sumOfConvRates += stagesCounter - neededStagesOfStability;
					}

					double averageFuncBestValue = sumOfFuncBestValues / expIterations;
					double convergenceRate = sumOfConvRates / expIterations;

					param.researchResults.Add(new ResearchResult(
						Int32.Parse(counter.ToString()), 
						param.currentValue.ToString("0.0"),
						functionMin/averageFuncBestValue, 
						convergenceRate));
				}
			}

			PrintResearchResults();
			ResetControllerParametersValues();

			void PrintResearchResults()
			{
				foreach (var param in ControlledParameters)
				{
					Console.WriteLine("\n " + param.name + "\n");
					foreach (var result in param.researchResults)
						Console.WriteLine(
							result.expNumber + " " +
							result.paramValue + " " +
							result.solutionQuality + " " +
							result.convergenceRate);
				}
			}
			void ResetControllerParametersValues()
			{
				PopulationSize.currentValue = PopulationSize.minValue;
				PopulationSize.researchResults = new List<ResearchResult>();
				MutationChance.currentValue = MutationChance.maxValue;
				MutationChance.researchResults = new List<ResearchResult>();
				CrossingChance.currentValue = CrossingChance.maxValue;
				CrossingChance.researchResults = new List<ResearchResult>();
			}
		}
		public void PerformGeneticAlgorithm()
		{
			bestFunctionValue = double.NaN;
			stagesCounter = 0;
			int solutionStabilityCounter = 0;

			while (solutionStabilityCounter < neededStagesOfStability)
			{

				if (printGADetails) Console.WriteLine("\n Этап эволюции № " + stagesCounter);
				PerformOneStageOfEvolution();
				if (printGADetails) PrintAllPopulation();

				if (bestFunctionValue == double.NaN)
					bestFunctionValue = survivors[0].funcValue;
				else
				{
					double difference = Math.Abs(Convert.ToDouble(bestFunctionValue - survivors[0].funcValue));

					if (difference < E && difference >= 0)
						solutionStabilityCounter++;
					else
						solutionStabilityCounter = 0;

					bestFunctionValue = survivors[0].funcValue;
					//Console.WriteLine("пук" + bestFunctionValue);
				}
				ResetCollections();
				stagesCounter++;
			}
			population = null;

			//локальные функци
			void PerformOneStageOfEvolution()
			{
				if (population == null)
					CreatePopulation();

				switch (operatorsMode)
				{
					case OperatorsMode.defaultOperators:
						PerformCrossing_Default();
                        PerformMutation_Custom();
						PerformSelection_Default();
						break;

					
				}

			}
			void ResetCollections()
			{
				population = null;
				population = survivors;
				survivors = null;
				parentCouples = null;

				for (int i = 0; i < population.Count; i++)
				{
					population[i].Type = Type.parent;
					population[i].index = i;
				}
			}
		}	

		private void CreatePopulation()
		{
			int popSize =  Int32.Parse(PopulationSize.currentValue.ToString());

			population = new List<Individual>(popSize);
			Random rand = new Random();

            string[] lines = File.ReadAllLines(@"NumPos.txt");
            int[,] NumPos = new int[lines.Length, lines[0].Split(' ').Length];
            for (int i = 0; i < lines.Length; i++)
            {
                string[] temp = lines[i].Split(' ');
                for (int j = 0; j < temp.Length; j++)
                    NumPos[i, j] = Convert.ToInt32(temp[j]);
            }

            int[] pos = new int[NumPos.Length];
            for (int i = 0; i < NumPos.Length / 3; i++)
                pos[i] = NumPos[i, 0];

            int vr = NumPos.Length / 3;
            Random rnd = new Random();
            int[] array = Enumerable.Range(0, vr ).OrderBy(y => rnd.Next()).Take(vr).ToArray();
            for (int j = 0; j < popSize; j++)
			{

				int[] chromo_1 = new int[L];
                array = Enumerable.Range(0, vr).OrderBy(y => rnd.Next()).Take(vr).ToArray();

                for (int i = 0; i < L; i++)
				{				
					chromo_1[i] = array[i];
					
				}

				Individual dude = new Individual(population.Count, chromo_1, Type.parent);
				population.Add(dude);
			}
		}

		#region Определение родительских пар
		private void DefineParentCouplesDefault()
		{
			if(printGADetails) PrintBasicPopulation();

			if(printGADetails) Console.WriteLine("\nРодительские пары");

			int couplesCount = population.Count / 2;
			parentCouples = new List<Tuple<Individual, Individual>>(couplesCount);

			List<int> mixedItemsNumbers = new List<int>();
			Random rand = new Random();

			while (mixedItemsNumbers.Count < population.Count)
			{
				int temp = rand.Next(0, population.Count);
				if (!mixedItemsNumbers.Contains(temp))
					mixedItemsNumbers.Add(temp);
			}

			for (int i = 0; i < couplesCount; i++)
			{
				Tuple<Individual,Individual> temp =
					new Tuple<Individual, Individual>(population[mixedItemsNumbers[i]], population[mixedItemsNumbers[mixedItemsNumbers.Count - i - 1]]);
				parentCouples.Add(temp);

				if (printGADetails)
				{
					PrintItem(temp.Item1);
                    PrintItem(temp.Item2);
                }
			}
		}
		
		#endregion


		#region Скрещивание
		private void PerformCrossing_Default()
		{			
			int allowedCrossingCounts = int.Parse(Math.Round(PopulationSize.currentValue * CrossingChance.currentValue).ToString());
			DefineParentCouplesDefault();

			if(printGADetails) Console.WriteLine("\nПотомки");

            foreach (var parentCouple in parentCouples)
            {
                Tuple<int[], int[]> chromo_1_pair = Cross(parentCouple.Item1.chromosome_1, parentCouple.Item2.chromosome_1, allowedCrossingCounts);
                

                var dude = new Individual(population.Count, chromo_1_pair.Item1, Type.child);
                population.Add(dude);
                var dude_2 = new Individual(population.Count, chromo_1_pair.Item2, Type.child);
                population.Add(dude_2);

                if (printGADetails)
                {
                    PrintItem(dude);
                    PrintItem(dude_2);
                }
            }

            Tuple<int[], int[]> Cross(int[] chromo_1, int[] chromo_2, int allowedCrossings)
            {
                if (allowedCrossings <= 0)
                    return new Tuple<int[], int[]>(chromo_1, chromo_2);

                int crossingPoint = 4;
                int[] result_1 = new int[L];
                int[] result_2 = new int[L];
                List<int> vs = new List<int>();
                List<int> vs1 = new List<int>();

                for (int i = 0; i < crossingPoint; i++)
                {
                    result_1[i] = chromo_1[i];
                    result_2[i] = chromo_2[i];
                    
                }
                for (int i = 0; i < L; i++)
                {
                    vs.Add(chromo_2[i]);
                    vs1.Add(chromo_1[i]);
                }

                for (int i = 0; i < crossingPoint; i++)
                {
                    if (vs.Contains(chromo_1[i]))
                    { vs.Remove(chromo_1[i]); }
                    if (vs1.Contains(chromo_2[i]))
                    { vs1.Remove(chromo_2[i]); }
                }
   
                for(int i = crossingPoint; i < L; i++)
                {
                    result_1[i] = vs[i - 4];
                    result_2[i] = vs1[i - 4];
                }

                return new Tuple<int[], int[]>(result_1, result_2);
            };
        }
		
        
		#endregion


		#region Мутация

        private void PerformMutation_Custom()
		{
			if(printGADetails) Console.WriteLine("\nДвухточечная мутация");
            Random rand = new Random();
            for (int i = 0; i < population.Count; i++)
			{
				if (population[i].Type == Type.child)
				{
					Individual dude = new Individual(
						population.Count,
						Mutate(population[i].chromosome_1),
						Type.mutant);
					population.Add(dude);
					if(printGADetails) PrintItem(dude);
				}
			}

          
            int[] Mutate(int[] chromosome)
			{
                int Point = 0;
                int First = rand.Next(0,L);
                int Second = rand.Next(0, L);
                Point = chromosome[First];
                chromosome[First] = chromosome[Second];
                chromosome[Second] = Point;
                
                    return chromosome;
			}
		}
        #endregion


        #region Селекция
        private void PerformSelection_Default()
        {
            int survivorsNeededQuantity = 6;
            survivors = new List<Individual>();

            if (printGADetails) Console.WriteLine("\nРезультаты селекции");

            foreach (var dude in population)
                dude.GetSolution();

            population.Sort();

            if (population.Count < survivorsNeededQuantity)
                survivorsNeededQuantity = population.Count;

            for (int i = 0; i < survivorsNeededQuantity; i++)
                survivors.Add(population[i]);

            if (printGADetails) PrintSelectionResults();

        }
        class RankedIndividual: IComparable<RankedIndividual>
		{
			public double P;
			public int rank;
			public Individual individual;
			public RankedIndividual(double p, int rank, Individual ind)
			{
				this.P = p;
				this.rank = rank;
				this.individual = ind;
			}
			public int CompareTo(RankedIndividual ind)
			{
				return ind.P.CompareTo(this.P);
			}
		}
        class IntervalNode
        {
            public Individual dude;
            public double startPoint;
            public IntervalNode(Individual dude, double startPoint)
            {
                this.dude = dude;
                this.startPoint = startPoint;
            }
        }
        // Селекция на основе «колеса рулетки»
        private void PerformSelection_Custom()
        {
            if (printGADetails) Console.WriteLine("\nСелекция на основе «колеса рулетки»");

            int survivorsNeededQuantity = 6;
            survivors = new List<Individual>();
            Random rand = new Random();
            List<Individual> populationTemp = new List<Individual>();

            populationTemp.AddRange(population);

            foreach (var dude in populationTemp)
               dude.GetSolution();


            while (populationTemp.Count > survivorsNeededQuantity)
            {
                List<IntervalNode> intervalNodes = new List<IntervalNode>();
                //Console.WriteLine("\n Размер популяции: " + population.Count);
                double sumOfFuncValues = 0;

                foreach (var dude in populationTemp)
                    sumOfFuncValues += (1 / dude.funcValue);

                double summOfIntervals = 0;
                for (int i = 0; i < populationTemp.Count; i++)
                {
                    if (i == 0)
                    {
                        intervalNodes.Add(new IntervalNode(populationTemp[i], 0));
                        //Console.Write("0 -> ");
                    }
                    else
                    {
                        double newNod = summOfIntervals -= GetInterval(i - 1, sumOfFuncValues);
                        intervalNodes.Add(new IntervalNode(populationTemp[i], newNod));
                        //Console.Write(newNod.ToString("0.000") + "-> ");
                    }
                }

                double shot = (double)rand.Next(1, 11) / 10;
                //Console.WriteLine("\nshot = " + shot);

                for (int i = 0; i < intervalNodes.Count; i++)
                {
                    double startPoint = intervalNodes[i].startPoint;
                    double endPoint;

                    if (i != intervalNodes.Count - 1)
                        endPoint = intervalNodes[i + 1].startPoint;
                    else
                        endPoint = 1;

                    if (shot > startPoint && shot < endPoint)
                    {
                        //Console.WriteLine($"stP({startPoint}) - shot({shot}) - enP({endPoint})");
                        //Console.WriteLine("Убил!");
                        populationTemp.Remove(intervalNodes[i].dude);
                        
                        break;
                    }
                }
            }

            populationTemp.Sort();
            survivors = populationTemp;
            if (printGADetails) PrintSelectionResults();

            double GetInterval(int i, double sum)
            {
                return (1 / (populationTemp[i].funcValue * sum));
            }
        }
        #endregion


        #region Вывод в консоль

        private void PrintBasicPopulation()
		{
			Console.WriteLine("\nНачальная популяция");
			foreach (var dude in population)
				PrintItem(dude);
		}
		private void PrintSelectionResults()
		{
			foreach (var dude in survivors)
			{
				foreach (var num in dude.chromosome_1)
				Console.Write(num + " ");
				Console.WriteLine("| " + dude.funcValue + "| " + dude.index);
			}
				
		}
		private void PrintAllPopulation()
		{
			Console.WriteLine("\nРасчет значений целевой функции");
			foreach (var dude in population)
			{
				Console.Write(
					dude.index + " | " +
					dude.chromoDecimalValue_1 + " | " +
					dude.funcValue);
				Console.WriteLine();
			}
		}
		private void PrintItem(Individual dude)
		{
			foreach (var num in dude.chromosome_1)
			Console.Write(num + " ");
            Console.Write("| " + dude.funcValue + " ");
            Console.WriteLine("| " + dude.index);
         
        }


		#endregion



	}
}
