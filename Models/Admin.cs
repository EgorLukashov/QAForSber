namespace QAForSber.Models
{
    public class Admin
    {
        public int IDAdmin { get; set; }

        public string AdminLogin { get; set; }

        public string AdminName { get; set; }

        public string AdminSurname { get; set; }

        public string? AdminMiddleName { get; set; }

        public string AdminHashedPassword { get; set; }

        public string AdminEmail { get; set; }

        public class KafkaMessage<T>
        {
            public string Operation { get; set; }
            public T Data { get; set; }
        }
    }
}
