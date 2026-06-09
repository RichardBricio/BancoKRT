using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.Model;
using BancoKRT.Domain.Entities;
using BancoKRT.Domain.Interfaces;
using BancoKRT.Domain.ValueObjects;

namespace BancoKRT.Infrastructure.Repositories;

public class DynamoDBRepository : IAccountRepository
{
    private readonly IAmazonDynamoDB _client;
    private const string TABLE_NAME = "AccountLimits";

    public DynamoDBRepository()
    {
        _client = new AmazonDynamoDBClient("dummy", "dummy", new AmazonDynamoDBConfig
        {
            ServiceURL = "http://localhost:8000",
            UseHttp = true
        });
        
        EnsureTableExists().Wait();
    }

    private async Task EnsureTableExists()
    {
        var tables = await _client.ListTablesAsync();
        if (tables.TableNames.Contains(TABLE_NAME)) return;

        var request = new CreateTableRequest
        {
            TableName = TABLE_NAME,
            AttributeDefinitions = new List<AttributeDefinition>
            {
                new AttributeDefinition { AttributeName = "PK", AttributeType = ScalarAttributeType.S },
                new AttributeDefinition { AttributeName = "GSI_PK", AttributeType = ScalarAttributeType.S }
            },
            KeySchema = new List<KeySchemaElement>
            {
                new KeySchemaElement { AttributeName = "PK", KeyType = KeyType.HASH }
            },
            GlobalSecondaryIndexes = new List<GlobalSecondaryIndex>
            {
                new GlobalSecondaryIndex
                {
                    IndexName = "DocumentIndex",
                    KeySchema = new List<KeySchemaElement>
                    {
                        new KeySchemaElement { AttributeName = "GSI_PK", KeyType = KeyType.HASH }
                    },
                    Projection = new Projection { ProjectionType = ProjectionType.ALL }
                }
            },
            BillingMode = BillingMode.PAY_PER_REQUEST
        };

        await _client.CreateTableAsync(request);
    }

    public async Task<AccountLimit?> GetByAgencyAndAccountAsync(AgencyNumber agency, AccountNumber account)
    {
        var pk = $"{agency.Value}#{account.Value}";
        
        var response = await _client.GetItemAsync(TABLE_NAME, new Dictionary<string, AttributeValue>
        {
            { "PK", new AttributeValue { S = pk } }
        });

        if (!response.IsItemSet) return null;

        return HydrateAccount(response.Item);
    }

    public async Task<IEnumerable<AccountLimit>> GetByDocumentAsync(Cpf document)
    {
    var request = new QueryRequest
    {
        TableName = TABLE_NAME,
        IndexName = "DocumentIndex",
        KeyConditionExpression = "GSI_PK = :gsiPk",
        ExpressionAttributeValues = new Dictionary<string, AttributeValue>
        {
            { ":gsiPk", new AttributeValue { S = $"DOC#{document.Value}" } }
        }
    };

    var response = await _client.QueryAsync(request);
    var results = new List<AccountLimit>();

    foreach (var item in response.Items)
    {
        results.Add(HydrateAccount(item));
    }

    return results;
}

    public async Task<IEnumerable<AccountLimit>> GetAllAsync()
    {
        var response = await _client.ScanAsync(new ScanRequest { TableName = TABLE_NAME });
        var results = new List<AccountLimit>();

        foreach (var item in response.Items)
        {
            results.Add(HydrateAccount(item));
        }

        return results;
    }

    public async Task AddAsync(AccountLimit account)
    {
        var item = DehydrateAccount(account);
        await _client.PutItemAsync(TABLE_NAME, item);
    }

    public async Task UpdateAsync(AccountLimit account)
    {
        var item = DehydrateAccount(account);
        await _client.PutItemAsync(TABLE_NAME, item);
    }

    public async Task DeleteAsync(AgencyNumber agency, AccountNumber account)
    {
        var pk = $"{agency.Value}#{account.Value}";
        await _client.DeleteItemAsync(TABLE_NAME, new Dictionary<string, AttributeValue>
        {
            { "PK", new AttributeValue { S = pk } }
        });
    }

    public async Task<bool> ExistsAsync(AgencyNumber agency, AccountNumber account)
    {
        var result = await GetByAgencyAndAccountAsync(agency, account);
        return result != null;
    }

    // Mapeamento para DynamoDB (Hydrate = carregar do banco)
    private AccountLimit HydrateAccount(Dictionary<string, AttributeValue> item)
    {
        return new AccountLimit(
            new Cpf(item["Document"].S),
            new AgencyNumber(item["AgencyNumber"].S),
            new AccountNumber(item["AccountNumber"].S),
            decimal.Parse(item["PixLimit"].N, System.Globalization.CultureInfo.InvariantCulture),
            DateTime.Parse(item["CreatedAt"].S),
            DateTime.Parse(item["UpdatedAt"].S)
        );
    }

    // Mapeamento para DynamoDB (Dehydrate = salvar no banco)
    private Dictionary<string, AttributeValue> DehydrateAccount(AccountLimit account)
    {
        return new Dictionary<string, AttributeValue>
        {
            { "PK", new AttributeValue { S = account.PK } },
            { "GSI_PK", new AttributeValue { S = account.GSI_PK } },
            { "Document", new AttributeValue { S = account.Document.ToString() } },
            { "AgencyNumber", new AttributeValue { S = account.AgencyNumber.ToString() } },
            { "AccountNumber", new AttributeValue { S = account.AccountNumber.ToString() } },
            { "PixLimit", new AttributeValue { N = account.GetLimit().ToString(System.Globalization.CultureInfo.InvariantCulture) } },
            { "CreatedAt", new AttributeValue { S = account.CreatedAt.ToString("yyyy-MM-dd HH:mm:ss") } },
            { "UpdatedAt", new AttributeValue { S = account.UpdatedAt.ToString("yyyy-MM-dd HH:mm:ss") } }
        };
    }
}