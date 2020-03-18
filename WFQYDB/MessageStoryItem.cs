using System;

namespace WFQYDB
{
    public class MessageStoryItem: Message
    {
        public DateTime DateTime { get; private set; }

        public enum Direction {Rx, Tx }

        public Direction Dir { get; set; }

        public MessageStoryItem(Direction dir, Message message)
        {
            DateTime = DateTime.Now;
            FitBuffer(message.FullLengthWithCrc);
            message.Buffer.Take(0, message.FullLengthWithCrc).CopyTo(buffer, 0);
            Dir = dir;
        }
    }
}
