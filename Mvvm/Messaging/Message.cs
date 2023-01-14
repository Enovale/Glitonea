using System;
using System.Collections.Generic;
using System.Linq;

namespace Glitonea.Mvvm.Messaging
{
    public class Message
    {
        private static Dictionary<object, Dictionary<Type, MulticastDelegate>> _recipients = new();

        public static void Subscribe<T>(object recipient, Action<T> handler)
        {
            if (!_recipients.ContainsKey(recipient))
            {
                _recipients.Add(recipient, new());
            }

            _recipients[recipient].Add(typeof(T), handler);
        }

        public static void Unsubscribe<T>(object recipient)
        {
            if (_recipients.ContainsKey(recipient))
            {
                if (_recipients[recipient].ContainsKey(typeof(T)))
                    _recipients[recipient].Remove(typeof(T));
            }
        }

        public static void Broadcast<T>(T message) where T : Message
            => PushToSubscribers(message);
        
        public static void Broadcast<T>() where T : Message, new()
            => PushToSubscribers(new T());

        public static void BroadcastToType<T, U>(T message) where T : Message
            => PushToSubscribers(message, typeof(U));
        
        public static void BroadcastToType<T, U>() where T : Message, new()
            => PushToSubscribers(new T(), typeof(U));

        public void Broadcast()
            => PushToSubscribers(this);

        public void BroadcastToType<U>()
            => PushToSubscribers(this, typeof(U));

        private static void PushToSubscribers(object message)
        {
            var t = message.GetType();

            foreach (var kvp in _recipients)
            {
                if (kvp.Value.ContainsKey(t))
                {
                    kvp.Value[t].Method.Invoke(kvp.Key, new[] { Activator.CreateInstance(t, new object[] { }) });
                }
            }
        }
        
        private static void PushToSubscribers(object message, Type subscriberType)
        {
            var t = message.GetType();

            foreach (var kvp in _recipients.Where(x => subscriberType.IsAssignableFrom(x.GetType())))
            {
                if (kvp.Value.ContainsKey(t))
                {
                    kvp.Value[t].Method.Invoke(kvp.Key, new[] { Activator.CreateInstance(t, new object[] { }) });
                }
            }
        }
    }
}