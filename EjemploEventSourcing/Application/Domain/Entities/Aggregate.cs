using System;
using System.Collections.Generic;
using System.Linq;
using EjemploEventSourcing.Application.Domain.Events.Interfaces;

namespace EjemploEventSourcing.Application.Domain.Entities
{
    public abstract class Aggregate : IInitAggregate, ITraceChangesInAgrregate
    {
        const int INITIAL_VERSION = 0;

        private string _id;
        private int _baseVersion;
        private int _actualVersion;
        private IList<IEvent> _events;

        protected string GetAggregateId()
        {
            return _id;
        }

        protected int GetAggregateVersion()
        {
            return _actualVersion;
        }

        protected string GetClassName()
        {
            return GetType().Name;
        }

        protected void Init(string id)
        {
            //Used when you're creating an aggregate yourself
            _id = id;
            _baseVersion = INITIAL_VERSION;
            _actualVersion = INITIAL_VERSION;
            _events = new List<IEvent>();
        }

        protected void ApplyEvent(IEvent e)
        {
            ReproduceEvent(e);
            AddEvent(e);
            _actualVersion++;
        }

        public abstract void ReproduceEvent(IEvent e);

        private void AddEvent(IEvent e)
        {
            //Records the event
            _events.Add(e);
        }

        public void BuildAggregate(IAggregateInfoConstructor data)
        {
            //Used when you read an aggregate from the event store
            _baseVersion = data.AggregateBaseVersion;
            _events = new List<IEvent>();

            foreach (IEvent e in data.Events)
            {
                ReproduceEvent(e);
                _actualVersion++;
            }

            if (_baseVersion != _actualVersion)
                throw new InvalidOperationException($"The base version does not match with the actual version of the aggregate - Base: {_baseVersion} - Actual: {_actualVersion}");
        }

        public bool HasChanges()
        {
            return _baseVersion != _actualVersion;
        }

        public IChangesInAggregateInfo GetChanges()
        {
            //Used when you write an aggregate to the event store
            var aggregateInfo = new AggregateInfo
            {
                AggregateId = _id,
                AggregateBaseVersion = _baseVersion,
                AggregateActualVersion = _actualVersion,
                AggregateType = GetClassName()
            };

            var changes = new ChangesInAggregateInfo
            {
                AggregateInfo = aggregateInfo,
                Events = _events
            };

            return changes;
        }

        public void AcceptChanges()
        {
            _baseVersion = _actualVersion;
            _events = new List<IEvent>();
        }
    }
}