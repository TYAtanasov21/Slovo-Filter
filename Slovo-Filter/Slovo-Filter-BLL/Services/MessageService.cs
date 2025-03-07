using Slovo_Filter_DAL.Models;
using Slovo_Filter_DAL.Repositories;

namespace Slovo_Filter_BLL.Services;
public class MessageService
{
    private readonly MessageRepository _messageRepository;

    public MessageService()
    {
        _messageRepository = new MessageRepository();
    }

    public async Task<IEnumerable<Message>> GetMessageHistoryAsync(string user1Id, string user2Id, int limit)
    {
        if (!int.TryParse(user1Id, out int user1IdInt) || !int.TryParse(user2Id, out int user2IdInt))
        {
            throw new ArgumentException("User IDs must be valid integers");
        }

        return await _messageRepository.GetMessageHistoryAsync(user1IdInt, user2IdInt, limit);
    }
    
    public async Task<int> StoreMessageAsync(int senderId, int receiverId, string content, int aiScore)
    {
        return await _messageRepository.StoreMessageAsync(senderId, receiverId, content, aiScore);
    }

}