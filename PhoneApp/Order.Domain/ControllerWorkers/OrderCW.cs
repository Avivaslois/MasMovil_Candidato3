﻿using System;
using System.Collections.Generic;
using System.Text;
using Order.Domain.DTO;
using System.Linq;
using Order.Infrastructure.APIHelper;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;

namespace Order.Domain.ControllerWorkers
{
    public class OrderCW : IOrderCW
    {
        private APIHelper apiHelper;
        private IOptions<PhoneApiConfiguration> settings;
        public OrderCW(IOptions<PhoneApiConfiguration> settings)
        {
            //Settings loaded by DI on Order.API startup file
            this.settings = settings;
            var url = settings.Value.url;
            if(string.IsNullOrEmpty(url))
            {
                url = "http://127.0.0.1:8081/api/";
            }
            
            
            apiHelper = new APIHelper(url);
        }
        public OrderCW(APIHelper mockedAPI)
        {
            apiHelper = mockedAPI;
        }

        public OrderCW() { }

        public async Task<bool> processOrder(OrderDto order)
        {
            var prices = await getPhonePrices(order.OrderDetails);

            prices = completeResultInfo(order, prices);

            var message = compositeMessage(order.Customer, prices);

            Console.Write(message);

            return true;
        }

        public async Task<List<OrderDetailResultDto>> getPhonePrices(List<OrderDetailDto> details)
        {
            List<int> phonesIds = details.Select(x => x.PhoneId).Distinct().ToList();

            var response = await apiHelper.Post<List<OrderDetailResultDto>>("phones", phonesIds);

            return response;
        }

        public List<OrderDetailResultDto> completeResultInfo(OrderDto order, List<OrderDetailResultDto> detailsResults)
        {
            try
            {
                detailsResults.ForEach(x => {
                    var originalOD = order.OrderDetails.FirstOrDefault(y => y.PhoneId == x.PhoneId);
                    x.Quantity = originalOD.Quantity;
                    x.PhoneName = originalOD.PhoneName;
                });

            }
            catch (Exception ex)
            {
                throw ex;
            }
            return detailsResults;
        }

        public string compositeMessage(CustomerDto customer, List<OrderDetailResultDto> detailsResults)
        {
            string message = string.Empty;
            var totalResult = detailsResults.Sum(x=> x.OrderDetailAmount);

            message += $@"{Environment.NewLine}{Environment.NewLine}{Environment.NewLine}
                          Customer Info: {customer.Name} {customer.Surname} ({customer.Email} 
                          {Environment.NewLine})
                          ------------------------------------------------
                          {Environment.NewLine}";

            foreach (var item in detailsResults)
            {

                message += $@"Order Detail: 
                                Phone Name: {item.PhoneName}
                                Quantity:   {item.Quantity}
                                Price:      {item.PhonePrice}
                                Subtotal:   {item.OrderDetailAmount} {Environment.NewLine}";                      
            }

            message += $"TOTAL AMOUNT: {totalResult}";

            return message;
        }
    }
}
