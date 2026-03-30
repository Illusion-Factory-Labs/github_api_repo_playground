
using API_Conta_SaldoExtrato.Config;
using Microsoft.Extensions.Options;
using StackExchange.Redis;

public  class RedisConnectionHelper
{
    private readonly CacheConfig _redisConfiguration;
    private readonly ILogger _logger;
    private IDatabaseAsync _databaseWaitListAsync;

    public RedisConnectionHelper(IOptions<CacheConfig> redisConfiguration, ILogger<RedisConnectionHelper> logger)
    {
        _redisConfiguration = redisConfiguration.Value;
        _logger = logger;
        createDataBase();
    }

    private void createDataBase()
    {
        var connectionMultiplexer = ConnectionMultiplexer.Connect(_redisConfiguration.Connection);

        _databaseWaitListAsync = connectionMultiplexer.GetDatabase(0);

    }

    public async Task<bool> isInCache(string token)
    {
        _logger.LogInformation($"RedisUtil - isInCache start. Registro : {token}");

        var key = new RedisKey(token);
        bool isInCache = await _databaseWaitListAsync.KeyExistsAsync(key);

        _logger.LogInformation($" Documento : {token} in cache: {isInCache}");
        return isInCache;
    }

    public async Task<string> getRegister(string chave)
    {

        RedisValue registroRedis = await _databaseWaitListAsync.StringGetAsync(chave);
        if (string.IsNullOrEmpty(registroRedis))
        {
            return null;
        }


        return registroRedis;
    }

    public async Task setRegister( string chave, string valor, int ttl = 0)
    {


        var value = new RedisValue(valor);
        var key = new RedisKey(chave);


        await _databaseWaitListAsync.StringSetAsync(key, value, TimeSpan.FromSeconds(ttl <= 0 ? _redisConfiguration.TTLPadrao : ttl));

    }
}
