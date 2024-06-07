using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PlannerAlgorithmsTesting
{
    internal class GreedyAlgorithm
    {
        public static Block[] PlanBlocks(Block[] blocks, TimeSlot[] timeSlots)
        {
            // Sort the blocks by their power consumption in descending order
            blocks = blocks.OrderByDescending(b => b.PowerConsumption * b.TimeSlotsNeeded).ToArray();

            var plannedBlocks = new List<Block>();

            var notPlannedBlocks = new List<Block>();

            // Iterate over each block
            for (int blockIndex = 0; blockIndex < blocks.Length; blockIndex++)
            {
                var block = blocks[blockIndex];

                // Iterate over each time slot in the day plan
                for (int i = 0; i < timeSlots.Length; i++)
                {
                    // Check if the current time slot has enough free power production
                    if (block.PowerConsumption <= timeSlots[i].PowerCapacity)
                    {
                        int j = i;
                        int duration = 0;

                        // Check if subsequent time slots can accommodate the block's duration
                        while (duration < block.TimeSlotsNeeded && j < timeSlots.Length)
                        {
                            if (block.PowerConsumption <= timeSlots[j].PowerCapacity)
                            {
                                // Deduct the block's power consumption from the current time slot
                                timeSlots[j].PowerCapacity -= block.PowerConsumption;
                                duration++;
                                j++;
                            }
                            else
                            {
                                // If not enough power, revert changes and break the loop
                                for (int x = i; x < j; x++)
                                {
                                    timeSlots[x].PowerCapacity += block.PowerConsumption;
                                }
                                break;
                            }
                        }

                        // If we have found enough consecutive time slots for the block
                        if (duration == block.TimeSlotsNeeded)
                        {
                            plannedBlocks.Add(new Block
                            {
                                Id = block.Id,
                                PowerConsumption = block.PowerConsumption,
                                TimeSlotsNeeded = block.TimeSlotsNeeded,
                                StartTimeSlotIndex = i
                            });
                            break; // Break the outer loop and move to the next block
                        }
                    }
                }
                // If the block was not planned, add it to the list of not planned blocks
                if (plannedBlocks.Count == 0 || plannedBlocks[plannedBlocks.Count - 1].Id != block.Id)
                {
                    notPlannedBlocks.Add(block);
                }
            }

            // If there are any not planned blocks, plan them using the NaiveAlgorithm
            if (notPlannedBlocks.Count > 0)
            {
                var plannedNotPlannedBlocks = NaiveAlgorithm.PlanBlocks(notPlannedBlocks.ToArray(), timeSlots);
                plannedBlocks.AddRange(plannedNotPlannedBlocks);
            }

            return plannedBlocks.ToArray();
        }
    }
}
