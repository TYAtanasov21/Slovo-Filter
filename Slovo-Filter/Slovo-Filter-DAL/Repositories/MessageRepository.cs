using System;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;
using Slovo_Filter_DAL.Models;

namespace Slovo_Filter_DAL.Repositories
{
    public class MessageRepository
    {
        private readonly dbContext _context;

        public MessageRepository()
        {
            _context = new dbContext();
        }
        
        public async Task<int> StoreMessageAsync(int sender, int receiver, string messageContent)
        {
            var query = @"
                    INSERT INTO messages (senderid, receiverid, content, timestamp, delivered)
                    VALUES (@senderid, @receiverid, @content, @timestamp, false) 
                    RETURNING id;";
                
            var parameters = new Dictionary<string, object>
            {
                { "@senderid", sender },
                { "@receiverid", receiver },
                { "@content", messageContent },
                { "@timestamp", DateTime.UtcNow }
            };

            var dataTable = await _context.ExecuteQueryAsync(query, parameters);
            if (dataTable.Rows.Count > 0)
            {
                return Convert.ToInt32(dataTable.Rows[0]["id"]);
            }

            return -1;
        }
        
        public async Task<List<Message>> GetOfflineMessagesAsync(int receiverId)
        {
            var query = "SELECT id, senderid, receiverid, content, timestamp, delivered FROM messages WHERE receiverid = @receiverid AND delivered = false;";
            var parameters = new Dictionary<string, object> { { "@receiverid", receiverId } };

            var dataTable = await _context.ExecuteQueryAsync(query, parameters);

            var messages = new List<Message>();

            foreach (DataRow row in dataTable.Rows)
            {
                messages.Add(new Message
                {
                    Id = Convert.ToInt32(row["id"]),
                    SenderId = Convert.ToInt32(row["senderid"]),
                    ReceiverId = Convert.ToInt32(row["receiverid"]),
                    Content = row["content"].ToString(),
                    Date = Convert.ToDateTime(row["timestamp"]),
                    IsDelivered = Convert.ToBoolean(row["delivered"])
                });
            }
            return messages;
        }
        
        public async Task<List<Message>> GetMessageHistoryAsync(int user1Id, int user2Id, int limit)
        {
            var query = @"
                SELECT id, senderid, receiverid, content, timestamp, delivered 
                FROM messages 
                WHERE (senderid = @user1id AND receiverid = @user2id) 
                   OR (senderid = @user2id AND receiverid = @user1id)
                ORDER BY timestamp DESC
                LIMIT @limit;";
                
            var parameters = new Dictionary<string, object> 
            { 
                { "@user1id", user1Id },
                { "@user2id", user2Id },
                { "@limit", limit }
            };

            var dataTable = await _context.ExecuteQueryAsync(query, parameters);

            var messages = new List<Message>();

            foreach (DataRow row in dataTable.Rows)
            {
                messages.Add(new Message
                {
                    Id = Convert.ToInt32(row["id"]),
                    SenderId = Convert.ToInt32(row["senderid"]),
                    ReceiverId = Convert.ToInt32(row["receiverid"]),
                    Content = row["content"].ToString(),
                    Date = Convert.ToDateTime(row["timestamp"]),
                    IsDelivered = Convert.ToBoolean(row["delivered"])
                });
            }
            return messages;
        }
        
        public async Task<bool> MarkMessageAsDeliveredAsync(int messageId)
        {
            var query = "UPDATE messages SET delivered = true WHERE id = @id;";
            var parameters = new Dictionary<string, object> { { "@id", messageId } };

            var rowsAffected = await _context.ExecuteNonQueryAsync(query, parameters);
            return rowsAffected > 0;
        }
    }
}
