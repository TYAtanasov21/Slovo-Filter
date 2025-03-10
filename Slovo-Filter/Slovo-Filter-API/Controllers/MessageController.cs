using Microsoft.AspNetCore.Mvc;
using Slovo_Filter_DAL.Repositories;
using System.Threading.Tasks;

namespace Slovo_Filter_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class MessagesController : ControllerBase
    {
        private readonly MessageRepository _messageRepository;

        public MessagesController()
        {
            _messageRepository = new MessageRepository();
        }

        [HttpPost]
        public async Task<IActionResult> StoreMessage([FromBody] StoreMessageRequest request)
        {
            Console.WriteLine("Storing message in the back-end");
            if (request.SenderId == 0 || request.ReceiverId == 0 || string.IsNullOrEmpty(request.Content))
            {
                return BadRequest("SenderId, ReceiverId, and Content are required.");
            }

            var messageId = await _messageRepository.StoreMessageAsync(request.SenderId, request.ReceiverId, request.Content, request.AiScore);

            if (messageId > 0)
                return Ok(new { MessageId = messageId });

            return StatusCode(500, "Failed to store message.");
        }

        [HttpGet("unread/{receiverId}")]
        public async Task<IActionResult> GetUnreadMessages(int receiverId)
        {
            Console.WriteLine("Getting unread messages");
            var messages = await _messageRepository.GetOfflineMessagesAsync(receiverId);

            return Ok(messages);
        }
        
        [HttpGet("history")]
        public async Task<IActionResult> GetMessageHistory([FromQuery] int user1Id, [FromQuery] int user2Id, [FromQuery] int limit = 50)
        {
            try
            {
                Console.WriteLine("Getting history");
                var messages = await _messageRepository.GetMessageHistoryAsync(user1Id, user2Id, limit);
                return Ok(messages);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error fetching message history: {ex.Message}");
                return StatusCode(500, "An error occurred while fetching message history.");
            }
        }

        [HttpPut("{messageId}/deliver")]
        public async Task<IActionResult> MarkAsDelivered(int messageId)
        {
            Console.WriteLine("Marking message as delivered");
            var success = await _messageRepository.MarkMessageAsDeliveredAsync(messageId);

            if (success)
                return Ok(new { Success = true });

            return NotFound("Message not found.");
        }
    }

    public class StoreMessageRequest
    {
        public int SenderId { get; set; }
        public int ReceiverId { get; set; }
        public string Content { get; set; }
        public int AiScore { get; set; } 
    }
}
