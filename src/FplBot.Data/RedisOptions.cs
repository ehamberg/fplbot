using System;

namespace FplBot.Data
{
    public class RedisOptions
    {
        public string REDIS_URL { get; set; } // Set by Heroku
        public string GetRedisPassword => RedisUri().UserInfo.Split(":")[1];

        public string GetRedisUsername => RedisUri().UserInfo.Split(":")[0];

        public string GetRedisServerHostAndPort => REDIS_URL.Split("@")[1];

        private Uri _uri;

        private Uri RedisUri()
        {
            if (_uri == null)
            {
                _uri = new Uri(REDIS_URL);
            }
            return _uri;
        }
    }
}