﻿using DeliverySystem.ClientUI.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.DTO.OrderDtos
{
    public class DriverPagedDTO:PersonDTO
    {
        public decimal CommissionRate { get; set; }
        public Shift Shift { get; set; }
    }
}
