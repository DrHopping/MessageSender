using MessageSender.Models;
using Microsoft.AspNetCore.Mvc;
using Twilio.Clients;
using Twilio.Rest.Api.V2010.Account;
using Twilio.Types;

namespace MessageSender.Services
{
    public class SmsSender : ISmsSender
    {
        private readonly ITwilioRestClient _client;
        public SmsSender(ITwilioRestClient client)
        {
            _client = client;
        }
        public void SendSms(SmsMessage model)
        {
            var message = MessageResource.Create(
                to: new PhoneNumber(model.To),
                from: new PhoneNumber(model.From),
                body: model.Message,
                client: _client);
        }
    }

    public interface ISmsSender
    {
        public void SendSms(SmsMessage model);
    }
}