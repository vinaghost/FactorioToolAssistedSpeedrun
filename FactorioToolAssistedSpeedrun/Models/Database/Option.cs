using FactorioToolAssistedSpeedrun.Enums;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace FactorioToolAssistedSpeedrun.Models.Database
{
    [ComplexType]
    public class Option
    {
        public OrientationType Orientation { get; set; }

        public InventoryType Inventory { get; set; }

        public Priority? Priority { get; set; }
    }
}