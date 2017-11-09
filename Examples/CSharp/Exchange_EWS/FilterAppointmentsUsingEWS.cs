﻿using Aspose.Email.Calendar;
using Aspose.Email.Clients.Exchange;
using Aspose.Email.Clients.Exchange.WebService;
using Aspose.Email.Tools.Search;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

/*
This project uses Automatic Package Restore feature of NuGet to resolve Aspose.Email for .NET API reference 
when the project is build. Please check https://Docs.nuget.org/consume/nuget-faq for more information. 
If you do not wish to use NuGet, you can manually download Aspose.Email for .NET API from http://www.aspose.com/downloads, 
install it and then add its reference to this project. For any issues, questions or suggestions 
please feel free to contact us using http://www.aspose.com/community/forums/default.aspx
*/

namespace Aspose.Email.Examples.CSharp.Email.Exchange_EWS
{
    class FilterAppointmentsUsingEWS
    {
        public static void Run()
        {
            // Connect to EWS
            const string mailboxUri = "https://outlook.office365.com/ews/exchange.asmx";
            const string username = "username";
            const string password = "password";
            const string domain = "domain";

            //ExStart: FilterAppointmentsByDateUsingEWS
            IEWSClient client = EWSClient.GetEWSClient(mailboxUri, username, password, domain);

            DateTime startTime = new DateTime(2017,09, 15);
            DateTime endTime = new DateTime(2017, 10, 10);
            ExchangeQueryBuilder builder = new ExchangeQueryBuilder();
            builder.Appointment.Start.Since(startTime);
            builder.Appointment.End.BeforeOrEqual(endTime);
            MailQuery query = builder.GetQuery();
            Appointment[] appointments = client.ListAppointments(query);
            //ExEnd: FilterAppointmentsByDateUsingEWS

            //ExStart: FilterAppointmentsByRecurrenceUsingEWS
            builder = new ExchangeQueryBuilder();
            builder.Appointment.IsRecurring.Equals(false);
            query = builder.GetQuery();
            appointments = client.ListAppointments(query);
            //ExEnd: FilterAppointmentsByRecurrenceUsingEWS
        }
    }
}