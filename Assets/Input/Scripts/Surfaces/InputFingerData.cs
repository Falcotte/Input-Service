namespace AngryKoala.Input
{
    internal readonly struct InputFingerData
    {
        public int Id { get; }

        public bool IsMouse { get; }

        /// <summary>
        /// Null when the finger is ignored.
        /// </summary>
        public string OwnerName { get; }

        public InputFingerData(int id, bool isMouse, string ownerName)
        {
            Id = id;
            IsMouse = isMouse;
            OwnerName = ownerName;
        }
    }
}
