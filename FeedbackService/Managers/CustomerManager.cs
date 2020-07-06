﻿using CachingManager.Managers;
using FeedbackService.DataAccess.Models;
using FeedbackService.Managers.Interfaces;
using FeedbackService.Options;
using FeedbackService.StringConstants.Messages;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace FeedbackService.Managers
{
    public class CustomerManager : BaseManager, ICustomerManager
    {
        private readonly IRepository _repository;
        private readonly IOrderManager _orderManager;

        public CustomerManager(IRepository repository, IOrderManager ordermanager, IDistributedCacheManager distributedCacheManager, IOptions<CacheOptions> cacheOptions)
            : base(distributedCacheManager, cacheOptions)
        {
            _repository = repository;
            _orderManager = ordermanager;
        }

        public async Task<Customer> GetCustommerByIdAsync(long userId, CancellationToken cancellationToken)
        {
            var typeName = nameof(Customer);
            var cachedCustomeers = await GetFromCacheAsync<Customer>(typeName, userId.ToString(), CustomerErrorMessages.UnableToRetrieveCustomer, cancellationToken);
            Customer customer = default;

            //If the user is already in cache, we just return true
            if (cachedCustomeers != null)
            {
                customer = cachedCustomeers.Where(customer => customer.Sid == userId).FirstOrDefault();
                if (customer != null)
                {
                    return customer;
                }
            }

            customer = await _repository.GetAsync<Customer>(customer => customer.Sid == userId, cancellationToken);

            if (customer != null)
            {
                var customerList = new List<Customer> { customer };
                await SetCacheAsync(customerList, typeName, userId.ToString(), cancellationToken);
                return customer;
            }

            // if the cusomer is null, just throw this exception
            throw new ArgumentException(string.Format(CustomerErrorMessages.CustomerDoesNotExists, userId));
        }
    }
}
