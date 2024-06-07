namespace PlannerAlgorithmsTesting
{
    internal class NaiveAlgorithm
    {
        public static Block[] PlanBlocks(Block[] blocks, TimeSlot[] timeSlots)
        {
            blocks = blocks.OrderByDescending(b => b.PowerConsumption).ToArray();
            for (int i = 0; i < timeSlots.Length; i++)
            {
                blocks = RealTimeUpdate(blocks, timeSlots[i], i, timeSlots.Length);
            }
            return blocks;
        }
        public static Block[] RealTimeUpdate(Block[] blocks, TimeSlot current, int timeSlotIndex, int timeSlotsTotal)
        {
            // subtract power consumption of running blocks from the current power capacity
            foreach (var block in blocks)
            {
                if(block.StartTimeSlotIndex != null && block.StartTimeSlotIndex + block.TimeSlotsNeeded > timeSlotIndex)
                {
                    current.PowerCapacity -= block.PowerConsumption;
                }
            }
            for (int i = 0; i < blocks.Length; i++)
            {
                if (blocks[i].StartTimeSlotIndex == null)
                {
                    // If the block can be started at the current time slot or it is the last time slot where it can be started
                    if (blocks[i].PowerConsumption <= current.PowerCapacity || blocks[i].TimeSlotsNeeded + timeSlotIndex == timeSlotsTotal)
                    {
                        blocks[i].StartTimeSlotIndex = timeSlotIndex;
                        current.PowerCapacity -= blocks[i].PowerConsumption;
                    }
                }
            }
            return blocks;
        }
    }
}
