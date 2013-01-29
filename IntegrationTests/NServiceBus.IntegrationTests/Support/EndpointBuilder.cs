﻿namespace NServiceBus.IntegrationTests.Support
{
    using System;
    using System.Collections.Generic;

    public class EndpointBuilder : IEndpointBehaviorFactory
    {
        public EndpointBuilder()
        {
            behavior.Givens = new List<Action<IBus>>();
            behavior.Whens = new List<Action<IBus,BehaviorContext>>();
            behavior.EndpointMappings = new Dictionary<Type, Type>();
        }
     
        public EndpointBuilder AddMapping<T>(Type endpoint)
        {
            this.behavior.EndpointMappings.Add(typeof(T),endpoint);

            return this;
        }


        public EndpointBuilder Given(Action<IBus> action)
        {
            this.behavior.Givens.Add(action);

            return this;
        }

        EndpointBehavior CreateScenario()
        {
            behavior.BuilderType = GetType();

            return this.behavior;
        }


        public EndpointBuilder When(Action<IBus> func)
        {
            this.behavior.Whens.Add((bus,c)=>func(bus));

            return this;
        }

        public EndpointBuilder When<TContext>(Action<IBus,TContext> func) where TContext : BehaviorContext
        {
            this.behavior.Whens.Add((bus,c)=>func(bus,(TContext)c));

            return this;
        }

        public EndpointBuilder EndpointSetup<T>() where T : IEndpointSetupTemplate
        {
            return EndpointSetup<T>(c => { });
        }

        public EndpointBuilder EndpointSetup<T>(Action<Configure> configCustomization) where T: IEndpointSetupTemplate
        {
            behavior.GetConfiguration = (settings,routingTable) =>
                {
                    var config = ((IEndpointSetupTemplate)Activator.CreateInstance<T>()).GetConfiguration(settings, behavior, new ScenarioConfigSource(behavior, routingTable));

                    configCustomization(config);

                    return config;
                };

            return this;
        }



        EndpointBehavior IEndpointBehaviorFactory.Get()
        {
            return CreateScenario();
        }


        readonly EndpointBehavior behavior = new EndpointBehavior();
    }
}