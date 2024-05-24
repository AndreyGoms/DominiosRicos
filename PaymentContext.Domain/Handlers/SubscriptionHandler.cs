using Flunt.Notifications;
using PaymentContext.Domain.Commands;
using PaymentContext.Domain.Entities;
using PaymentContext.Domain.Enums;
using PaymentContext.Domain.Repositories;
using PaymentContext.Domain.Services;
using PaymentContext.Domain.ValueObjects;
using PaymentContext.Shared.Commands;
using PaymentContext.Shared.Handlers;

 namespace PaymentContext.Domain.Handler;

public class SubscriptionHandler : 
    Notifiable,
    IHandler<CreateBoletoSubscriptionCommnad>,
    IHandler<CreatePayPalSubscriptionCommnad>
{
    private readonly IStudentRepository _repository;
    private readonly IEmailService _emailService;

    public SubscriptionHandler(IStudentRepository repository, IEmailService emailService)
    {
        _repository = repository;
        _emailService = emailService;
    }

    public ICommandResult Handle(CreateBoletoSubscriptionCommnad command)
    {
        //Fail Fast Validation - Aqui é feito algumas validações verificadas antes mesmo de seguir, validacao varia
        //de acordo com a sua regra!
        command.Validate(); //Metodo existente no command
        if(command.Invalid)
        {
            AddNotifications(command);
            return new CommandResult (false, "Não foi possivel realizar sua assinatura");
        }

        //Verficar se o documento já esta cadastrado
        if(_repository.DocumentExists(command.Document))
            AddNotification("Document","Este CPF já está em uso");

        //Verficar se o e-mail já esta cadastrado
        if(_repository.DocumentExists(command.Document))
            AddNotification("Email","Este Email já está em uso");


        //Gerar as VOs, os Value Objects
        var name = new Name(command.FirstName,command.LastName);
        var document = new Document(command.Document,EDocumentType.CPF);
        var email = new Email(command.Email);
        var address = new Address(command.Street, command.Number, command.Neighborhood, command.City, command.State, command.Country, command.ZipCode);
        
        //Gerar as Entidades
        var student = new Student(name, document, email);
        var subscription = new Subscription(DateTime.Now.AddMonths(1));
        var payment = new BoletoPayment(
             command.BarCode,
                command.BoletoNumber,
                command.PaidDate,
                command.ExpireDate,
                command.Total,
                command.TotalPaid,
                command.Payer,
                new Document(command.PayerDocument, command.PayerDocumentType),
                address,
                email
        );

        //Relacionamento
        subscription.AddPayment(payment);
        student.AddSubscription(subscription);

        //Agrupar as Validacoes
        AddNotifications(name, document, email, address, email,student,subscription,payment);

        //Salvar as informacoes
        _repository.CreateSubscription(student);

        //Enviar email de boas vindas
        _emailService.Send(student.Name.ToString(), student.Email.Address,"Bem vindo", "Sua assinatura foi criada!");

        //retornar informacoes
        return new CommandResult(true, "Assinatura realizada com sucesso!!");
    }

    public ICommandResult Handle(CreatePayPalSubscriptionCommnad command)
    {
        // Verificar se Documento já está cadastrado
        if (_repository.DocumentExists(command.Document))
            AddNotification("Document", "Este CPF já está em uso");

        // Verificar se E-mail já está cadastrado
        if (_repository.EmailExists(command.Email))
            AddNotification("Email", "Este E-mail já está em uso");

        // Gerar os VOs
        var name = new Name(command.FirstName, command.LastName);
        var document = new Document(command.Document, EDocumentType.CPF);
        var email = new Email(command.Email);
        var address = new Address(command.Street, command.Number, command.Neighborhood, command.City, command.State, command.Country, command.ZipCode);

        // Gerar as Entidades
        var student = new Student(name, document, email);
        var subscription = new Subscription(DateTime.Now.AddMonths(1));
        // Só muda a implementação do Pagamento
        var payment = new PayPalPayment(
            command.TransactionCode,
            command.PaidDate,
            command.ExpireDate,
            command.Total,
            command.TotalPaid,
            command.Payer,
            new Document(command.PayerDocument, command.PayerDocumentType),
            address,
            email
        );

        // Relacionamentos
        subscription.AddPayment(payment);
        student.AddSubscription(subscription);

        // Agrupar as Validações
        AddNotifications(name, document, email, address, student, subscription, payment);

        // Salvar as Informações
        _repository.CreateSubscription(student);

        // Enviar E-mail de boas vindas
        _emailService.Send(student.Name.ToString(), student.Email.Address, "bem vindo ao balta.io", "Sua assinatura foi criada");

        // Retornar informações
        return new CommandResult(true, "Assinatura realizada com sucesso");
    }
}