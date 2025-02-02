using Centrifugo.Client;
using Centrifugo.Client.Abstractions;
using Centrifugo.Client.Options;
using Centrifugo.Sample.Models;
using Centrifugo.Sample.TokenProvider;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Threading.Tasks;
using Websocket.Client;

namespace Centrifugo.Sample
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        private string _channel = "test-channel";
        private string _user = "42";

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddControllersWithViews();

            services.AddOptions<AuthOptions>();
            services.AddSingleton<ICentrifugoTokenProvider, CentrifugoTokenProvider>();
            services.AddSingleton<IWebsocketClient>(sp => new WebsocketClient(new Uri("ws://localhost:8000/connection/websocket?format=protobuf")));
            services.AddSingleton(sp =>
            {
                var logger = sp.GetRequiredService<ILogger<CentrifugoClient>>();
                var ws = sp.GetRequiredService<IWebsocketClient>();
                var tokenProvider = sp.GetRequiredService<ICentrifugoTokenProvider>();

                var config = new CentrifugoClient.Config();
                config.GetToken = (s => { return await tokenProvider.ConnectionTokenAsync(s, "my name is client"); });
                ICentrifugoClient client = new CentrifugoClient(ws, config);

                client.OnConnect(e => 
                {
//                    logger.LogInformation("ClientID: {0}", e.ClientID);
                });
                // onError, OnDisconnect

                var subscriptionConfig = new Subscription.SubscriptionConfig();
                subscriptionConfig.Token = await tokenProvider.SubscriptionTokenAsync(_user, _channel);

                var subscription = client.CreateNewSubscription(_channel, subscriptionConfig);

                subscription.OnSubscribe(e =>
                {
                    logger.LogInformation("SUBSCRIBED! channel name: " + e.Channel);
                });
                subscription.OnUnsubscribe(e =>
                {
                    logger.LogInformation("UNSUBSCRIBED! channel name: " + e.Channel);
                });
                subscription.OnPublish(publishEvent =>
                {
                    if (publishEvent.Data != null)
                    {
                        var s = publishEvent.Data.ToStringUtf8();
                        var json = JsonConvert.DeserializeObject<TestMessage>(s);

                        logger.LogInformation("OK! data: " + json);
                    } else
                    {
                        logger.LogInformation("OK!");
                    }

                    return Task.CompletedTask;
                });

                Task.Run(async () =>
                {
                    await client.ConnectAsync();
                    await client.SubscribeAsync(subscription);
                }).GetAwaiter().GetResult();

                return client;
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }
            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllerRoute(
                    name: "default",
                    pattern: "{controller=Home}/{action=Index}/{id?}");
            });
        }
    }
}
