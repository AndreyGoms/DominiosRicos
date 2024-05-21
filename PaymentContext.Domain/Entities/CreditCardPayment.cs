using PaymentContext.Domain.ValueObjects;

namespace PaymentContext.Domain.Entities
{
    public class CreditCardPayment : Payment
    {
        public CreditCardPayment(string cardHolderName, string cardNumber, string lastTransectionNumber, 
            DateTime paidDate, 
            DateTime expireDate, 
            decimal total, 
            decimal totalPaid, 
            string payer, 
            Document document, 
            Address address, 
            Email email) : base( 
                paidDate,  
                expireDate,  
                total,  
                totalPaid,  
                payer,  
                document, 
                address,  
                email)
        {
            CardHolderName = cardHolderName;
            CardNumber = cardNumber;
            LastTransectionNumber = lastTransectionNumber;
        }

        public string CardHolderName { get; private set; } 
        public string CardNumber { get; private set; }
        public string LastTransectionNumber { get; private set; }
        
    }
}