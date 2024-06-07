using Google.OrTools.LinearSolver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PlannerAlgorithmsTesting
{
    internal class LinearProgrammingAlgorithm
    {
        public static Block[] PlanBlocks(Block[] blocks, TimeSlot[] timeSlots)
        {
            //Solver milp_solver = Solver.CreateSolver("CBC_MIXED_INTEGER_PROGRAMMING");
            Solver milp_solver = Solver.CreateSolver("SCIP");

            //Variables indicating power overflows in each time slot
            Variable[] tsOverflowVars = milp_solver.MakeNumVarArray(
                timeSlots.Length, 0, double.PositiveInfinity, "TimeSlotOverflows");

            //Offset time slots to prevent negative indexes set to the maximum number of time slots needed by an appliance
            int offset = blocks.Max(b => b.TimeSlotsNeeded);

            //2-D array where each variable means if an appliance starts at a time slot
            Variable[,] appliancesVars = new Variable[blocks.Length, timeSlots.Length + offset];

            //Create variables for each appliance and time slot
            for (int i = 0; i < blocks.Length; i++)
            {
                for (int j = 0; j < timeSlots.Length + offset; j++)
                {
                    appliancesVars[i, j] = milp_solver.MakeBoolVar("Appliance" + i + "TimeSlot" + (j - offset));
                }
            }

            //Constraint -1: First starting time slot of each appliance must be at least the time slot on offset index.
            for (int i = 0; i < blocks.Length; i++)
            {
                for (int j = 0; j < offset; j++)
                {
                    milp_solver.Add(appliancesVars[i, j] == 0);
                }
            }


            //Constraint 0: Last starting time slot of each appliance must be at most the last time slot minus the number
            //of time slots the appliance needs
            for (int i = 0; i < blocks.Length; i++)
            {
                for (int j = timeSlots.Length - blocks[i].TimeSlotsNeeded + 1; j < timeSlots.Length; j++)
                {
                    milp_solver.Add(appliancesVars[i, j + offset] == 0);
                }
            }

            //Constraint 1: Each appliance must start exactly once
            for (int i = 0; i < blocks.Length; i++)
            {
                Constraint c1 = milp_solver.MakeConstraint(1, 1);
                for (int j = 0; j < timeSlots.Length + offset; j++)
                {
                    c1.SetCoefficient(appliancesVars[i, j], 1);
                }
            }

            //Constraint 2: In each time slot the overflow variable is greater or equal to the sum of power consumptions
            //of appliances that are running in that time slot minus the power capacity of that time slot. And each
            //appliance must run in consecutive time slots for the number of time slots it needs.
            for (int j = offset; j < timeSlots.Length + offset; j++)
            {
                Constraint c2 = milp_solver.MakeConstraint(double.NegativeInfinity, timeSlots[j - offset].PowerCapacity);
                for (int i = 0; i < blocks.Length; i++)
                {
                    for (int k = 0; k < blocks[i].TimeSlotsNeeded; k++)
                    {
                        c2.SetCoefficient(appliancesVars[i, j - k], blocks[i].PowerConsumption);
                    }
                }
                c2.SetCoefficient(tsOverflowVars[j - offset], -1);
            }

            //Objective function: Minimize the sum of overflow variables
            Objective objective = milp_solver.Objective();
            for (int j = 0; j < timeSlots.Length; j++)
            {
                objective.SetCoefficient(tsOverflowVars[j], 1);
            }
            objective.SetMinimization();

            //Solve the problem
            //Console.WriteLine("Number of variables = " + milp_solver.NumVariables());
            //Console.WriteLine("Number of constraints = " + milp_solver.NumConstraints());
            //Console.WriteLine("Started solving the problem...");
            var resultStatus = milp_solver.Solve();

            if (resultStatus != Solver.ResultStatus.OPTIMAL)
            {
                throw new Exception("The problem does not have an optimal solution!");
            }
            //Console.WriteLine("Problem solved in " + milp_solver.WallTime() + " milliseconds");
            /*
            Console.WriteLine("Problem solved in " + milp_solver.WallTime() + " milliseconds");

            Console.WriteLine("Objective value = " + milp_solver.Objective().Value());

            for (int i = 0; i < tsOverflowVars.Length; i++)
            {
                Console.WriteLine("Time slot " + i + " overflow = " + tsOverflowVars[i].SolutionValue());
            }

            for (int i = 0; i < blocks.Length; i++)
            {
                for (int j = 0; j < timeSlots.Length + offset; j++)
                {
                    if (appliancesVars[i, j].SolutionValue() == 1)
                        Console.WriteLine("Appliance " + i + " starts at time slot " + (j - offset));
                }
            }
            */

            //Return planned blocks
            var plannedBlocks = new Block[blocks.Length];
            for (int i = 0; i < blocks.Length; i++)
            {
                for (int j = offset; j < timeSlots.Length + offset; j++)
                {
                    if (appliancesVars[i, j].SolutionValue() == 1)
                    {
                        plannedBlocks[i] = new Block
                        {
                            Id = blocks[i].Id,
                            PowerConsumption = blocks[i].PowerConsumption,
                            TimeSlotsNeeded = blocks[i].TimeSlotsNeeded,
                            StartTimeSlotIndex = j - offset
                        };
                    }
                }
            }

            return plannedBlocks;
        }
    }
}
