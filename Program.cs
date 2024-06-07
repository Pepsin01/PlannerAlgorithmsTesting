using Google.OrTools.LinearSolver;
using System.Linq;

namespace PlannerAlgorithmsTesting
{
    struct Block
    {
        public int Id { get; init; }
        public int PowerConsumption;
        public int TimeSlotsNeeded;
        public int? StartTimeSlotIndex;
    }
    struct TimeSlot
    {
        public double PowerCapacity;
    }


    static class Program
    {
        static void Main(string[] args)
        {

            var blocks = new Block[]
            {
                new Block { Id = 1, PowerConsumption = 630, TimeSlotsNeeded = 210 }, // pračka 630Wh/210min
                new Block { Id = 2, PowerConsumption = 1200, TimeSlotsNeeded = 180 }, // sušička 1200Wh/180min
                new Block { Id = 3, PowerConsumption = 800, TimeSlotsNeeded = 240 }, // myčka 800Wh/240min
                new Block { Id = 4, PowerConsumption = 6000, TimeSlotsNeeded = 720 }, // tepelné čerpadlo vytápění 6000Wh/720min
                //new Block { Id = 5, PowerConsumption = 85000, TimeSlotsNeeded = 510 }, // elektromobil 85000Wh/510min
                new Block { Id = 6, PowerConsumption = 12000, TimeSlotsNeeded = 360 }, // elektrický 200L bojler 12000Wh/360min
                //new Block { Id = 7, PowerConsumption = 100, TimeSlotsNeeded = 12 }, // lednice 1200Wh/3h
                //new Block { Id = 8, PowerConsumption = 50, TimeSlotsNeeded = 15 }, // mrazák 1200Wh/3h
                //new Block { Id = 9, PowerConsumption = 100, TimeSlotsNeeded = 30 }, // mikrovlnka 1200Wh/3h
                //new Block { Id = 10, PowerConsumption = 250, TimeSlotsNeeded = 8 }, // trouba 1200Wh/3h
            };


            double plannedTimeSpan = 960; // 960 minutes = 16 hours
            double timeSlotLength = 15; // 15 minutes per time slot
            
            // round the number of time slots to the nearest higher integer
            var totalTimeSlots = (int)Math.Ceiling(plannedTimeSpan / timeSlotLength);

            blocks = blocks.Select(b => new Block
            {
                Id = b.Id,
                PowerConsumption = (int)Math.Ceiling(b.PowerConsumption / (b.TimeSlotsNeeded / timeSlotLength)),
                TimeSlotsNeeded = (int)Math.Ceiling(b.TimeSlotsNeeded / timeSlotLength)
            }).ToArray();

            /**/
            var totalBlocksPowerConsumption = blocks.Sum(b => b.PowerConsumption * b.TimeSlotsNeeded);
            var timer = System.Diagnostics.Stopwatch.StartNew();

            double timeElasedG = 0;
            double timeElasedL = 0;

            double overflowsH = 0;
            double overflowsN = 0;
            double overflowsG = 0;
            double overflowsL = 0;

            /*/
            var timeSlotsH = GenerateNormalDistributionValues(totalTimeSlots, totalBlocksPowerConsumption);
            Console.Write("{");
            for (int i = 0; i < timeSlotsH.Length; i++)
            {
                Console.Write($"{i}/{Math.Round(timeSlotsH[i].PowerCapacity)}, ");
            }
            Console.Write("}");
            /**/

            for (int i = 0; i < 100; i++)
            {
                var timeSlots = GenerateNormalDistributionValues(totalTimeSlots, totalBlocksPowerConsumption);

                var resultH = ByHandAlgorithm.PlanBlocks(blocks, timeSlots);
                overflowsH += CalculateTotalOverflows(resultH, timeSlots.ToArray());

                /**/
                var resultN = NaiveAlgorithm.PlanBlocks(blocks, timeSlots.ToArray());
                overflowsN += CalculateTotalOverflows(resultN, timeSlots.ToArray());

                timer.Restart();
                var resultG = GreedyAlgorithm.PlanBlocks(blocks, timeSlots.ToArray());
                timeElasedG += timer.ElapsedMilliseconds;
                overflowsG += CalculateTotalOverflows(resultG, timeSlots.ToArray());

                timer.Restart();
                var resultL = LinearProgrammingAlgorithm.PlanBlocks(blocks, timeSlots.ToArray());
                timeElasedL += timer.ElapsedMilliseconds;
                overflowsL += CalculateTotalOverflows(resultL, timeSlots.ToArray());

                Console.WriteLine($"{i + 1}% done.");
                /**/
            }
            Console.WriteLine($"By Hand Algorithm average overflows: {overflowsH / 100}");
            /**/
            Console.WriteLine($"Naive Algorithm average overflows: {overflowsN / 100}");
            Console.WriteLine($"Greedy Algorithm average overflows: {overflowsG / 100}");
            Console.WriteLine($"Greedy Algorithm average time: {timeElasedG / 100}ms");
            Console.WriteLine($"Linear Programming Algorithm average overflows: {overflowsL / 100}");
            Console.WriteLine($"Linear Programming Algorithm average time: {timeElasedL / 100}ms");
            /**/

            /*/
            for(int i = 0; i < timeSlots.Length; i++)
            {
                timeSlots[i].PowerCapacity /= 10;
            }
            for(int i = 0; i < timeSlots.Length; i++)
            {
                for(int j = 0; j < (int)(timeSlots[i].PowerCapacity); j++)
                {
                    Console.Write("*");
                }
                Console.WriteLine();
            }
            /**/

            /*/
            var timeSlots = GenerateNormalDistributionValues(64, totalBlocksPowerConsumption);
             
            Console.WriteLine("Naive Algorithm:");
            var resultN = NaiveAlgorithm.PlanBlocks(blocks, timeSlots.ToArray());
            PrintResults(resultN, timeSlots.ToArray());

            Console.WriteLine("\nGreedy Algorithm:");
            var resultG = GreedyAlgorithm.PlanBlocks(blocks, timeSlots.ToArray());
            PrintResults(resultG, timeSlots.ToArray());

            Console.WriteLine("\nLinear Programming Algorithm:");
            var resultL = LinearProgrammingAlgorithm.PlanBlocks(blocks, timeSlots.ToArray());
            PrintResults(resultL, timeSlots.ToArray());
            /**/
        }

        static TimeSlot[] GenerateNormalDistributionValues(int numberOfSlots, double totalCapacity)
        {
            Random rand = new Random();
            double mean = 0.5; // Mean of the distribution
            double standardDeviation = 0.17; // Standard deviation of the distribution

            double[] values = new double[numberOfSlots];
            for (int i = 0; i < numberOfSlots*100; i++)
            {
                double rndNormal;
                do
                {
                    // Using Box-Muller transform to generate normal distribution values
                    double u1 = 1.0 - rand.NextDouble();
                    double u2 = 1.0 - rand.NextDouble();
                    double randStdNormal = Math.Sqrt(-2.0 * Math.Log(u1)) * Math.Sin(2.0 * Math.PI * u2);

                    // Scale and shift to fit the desired mean and standard deviation
                    rndNormal = mean + standardDeviation * randStdNormal;
                } while (rndNormal < 0 || rndNormal > 1); // Ensure the value is within [0, 1]
                values[(int)(rndNormal * numberOfSlots)]++;
            }
            // Sum the values
            double sum = values.Sum();

            var timeSlots = new TimeSlot[numberOfSlots];

            // Normalize the values to sum to the totalElectricityProduced
            for (int i = 0; i < values.Length; i++)
            {
                timeSlots[i] = new TimeSlot
                {
                    PowerCapacity = values[i] / sum * totalCapacity
                };
            }
            return timeSlots;
        }
        static void PrintResults(Block[] result, TimeSlot[] timeSlots)
        {
            foreach (var block in result)
            {
                Console.WriteLine($"Block {block.Id} starts at time slot {block.StartTimeSlotIndex}");
            }
            Console.WriteLine($"Total overflows: {CalculateTotalOverflows(result, timeSlots)}");
        }
        static double CalculateTotalOverflows(Block[] result, TimeSlot[] timeSlots)
        {
            double totalOverflows = 0;
            for (int i = 0; i < timeSlots.Length; i++)
            {
                foreach (var block in result)
                {
                    if (block.StartTimeSlotIndex <= i && block.StartTimeSlotIndex + block.TimeSlotsNeeded > i)
                    {
                        timeSlots[i].PowerCapacity -= block.PowerConsumption;
                    }
                }
                if (timeSlots[i].PowerCapacity < 0)
                {
                    totalOverflows += -timeSlots[i].PowerCapacity;
                }
            }
            return totalOverflows;
        }
    }
}
