using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PlannerAlgorithmsTesting
{
    internal class ByHandAlgorithm
    {
        public static Block[] PlanBlocks(Block[] blocks, TimeSlot[] timeSlots)
        {
            return [
                new Block { Id = 1, PowerConsumption = 45, TimeSlotsNeeded = 14, StartTimeSlotIndex = 14 }, // pračka 630Wh/210min
                new Block { Id = 2, PowerConsumption = 100, TimeSlotsNeeded = 12, StartTimeSlotIndex = 46 }, // sušička 1200Wh/180min
                new Block { Id = 3, PowerConsumption = 50, TimeSlotsNeeded = 16, StartTimeSlotIndex = 12 }, // myčka 800Wh/240min
                new Block { Id = 4, PowerConsumption = 125, TimeSlotsNeeded = 48, StartTimeSlotIndex = 16 }, // tepelné čerpadlo vytápění 6000Wh/720min
                //new Block { Id = 5, PowerConsumption = 2500, TimeSlotsNeeded = 34, StartTimeSlotIndex = 16 }, // elektromobil 85000Wh/510min
                new Block { Id = 6, PowerConsumption = 500, TimeSlotsNeeded = 24, StartTimeSlotIndex = 28 } // elektrický 200L bojler 12000Wh/360min
            ];
        }
    }
}
