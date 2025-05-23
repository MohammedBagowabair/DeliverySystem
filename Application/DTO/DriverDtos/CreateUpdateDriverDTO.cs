﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Application.DTO.OrderDtos;
using DeliverySystem.ClientUI.Enums;

namespace Application.DTO.DriverDtos
{
    public class CreateUpdateDriverDTO : PersonDTO
    {
        public decimal CommissionRate { get; set; }
        public Shift Shift { get; set; }
        public List<OrderDTO>? Orders { get; set; }
        public string BloodType { get; set; } = string.Empty;
        public string EmergencyPhone { get; set; } = string.Empty;
        public string PassportNumber { get; set; } = string.Empty;
    }
}
