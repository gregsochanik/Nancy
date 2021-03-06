﻿namespace Nancy.Hosting.Wcf
{
    using System.Reflection;
    using System.ServiceModel;
    using System.ServiceModel.Channels;
    using System.ServiceModel.Web;
    using Nancy;
    using Nancy.Routing;

    [ServiceContract]
    [ServiceBehavior(InstanceContextMode = InstanceContextMode.Single)]
    public class NancyWcfGenericService
    {
        private readonly NancyEngine engine;

        public NancyWcfGenericService(Assembly modulesAssembly)
        {
            this.engine = new NancyEngine(new NancyModuleLocator(modulesAssembly), new RouteResolver());
        }

        [WebInvoke(UriTemplate="*")]
        public Message HandleOther()
        {
            return HandleAll();
        }

        [WebGet(UriTemplate="*")]
        public Message HandleGet()
        {
            return HandleAll();
        }

        private Message HandleAll()
        {
            var ctx = WebOperationContext.Current;
            
            var request = CreateNancyRequestFromIncomingRequest(ctx.IncomingRequest);
            var response = this.engine.HandleRequest(request);
            
            SetNancyResponseToOutgoingResponse(ctx.OutgoingResponse, response);

            return ctx.CreateStreamResponse(response.Contents, response.ContentType);
            //return ctx.CreateTextResponse(response.Contents);
        }

        private static IRequest CreateNancyRequestFromIncomingRequest(IncomingWebRequestContext req)
        {
            var ruri = req.UriTemplateMatch.BaseUri.MakeRelativeUri(req.UriTemplateMatch.RequestUri);
            return new Request(
                req.Method,
                "/"+ruri.ToString());
        }

        private static void SetNancyResponseToOutgoingResponse(OutgoingWebResponseContext resp, Response response)
        {
            resp.ContentType = response.ContentType;
            resp.StatusCode = response.StatusCode;
        }
    }
}
