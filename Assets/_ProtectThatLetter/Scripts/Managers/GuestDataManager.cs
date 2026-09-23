using UnityEngine;

namespace ProtectThatLetter.Managers {
    [DefaultExecutionOrder(-100)]
    public class GuestDataManager : MonoBehaviour {
        public static GuestDataManager Instance {
            get {
                if (_instance == null) {
                    _instance = FindFirstObjectByType<GuestDataManager>();
                    if (_instance == null) {
                        GameObject go = new GameObject("GuestDataManager");
                        _instance = go.AddComponent<GuestDataManager>();
                    }
                }
                return _instance;
            }
        }
        private static GuestDataManager _instance;

        private void Awake() {
            if (_instance != null && _instance != this) {
                Destroy(gameObject);
                return;
            }
            _instance = this;
            DontDestroyOnLoad(gameObject);
        }

        #region Properties
        public int InviteId { get; private set; }
        public string AccessCode { get; private set; } = string.Empty;
        public string GuestName { get; private set; } = string.Empty;
        public string GuestNickname { get; private set; }
        public string LetterContentVn { get; private set; } = string.Empty;
        public string LetterContentEn { get; private set; } = string.Empty;
        public string ImageUrl { get; private set; }
        public bool HasPassedGame { get; private set; } = false;
        public bool IsLetterRead { get; private set; } = false;
        public bool? IsAttending { get; private set; }
        public string GuestNote { get; private set; }
        #endregion

        #region Public Methods
        public void SaveGuestData(
            int inviteId,
            string code,
            string guestName,
            string guestNickname,
            string letterContentVn,
            string letterContentEn,
            string imageUrl,
            bool hasPassedGame,
            bool isLetterRead,
            bool? isAttending,
            string guestNote) {

            InviteId = inviteId;
            AccessCode = code;
            GuestName = guestName;
            GuestNickname = guestNickname;
            LetterContentVn = letterContentVn;
            LetterContentEn = letterContentEn;
            ImageUrl = imageUrl;
            HasPassedGame = hasPassedGame;
            IsLetterRead = isLetterRead;
            IsAttending = isAttending;
            GuestNote = guestNote;
        }

        public void UpdateGameCompletion(
            string contentVn,
            string contentEn,
            string imgUrl,
            bool isRead = false,
            bool? attending = null,
            string note = null) {

            HasPassedGame = true;
            LetterContentVn = contentVn;
            LetterContentEn = contentEn;
            ImageUrl = imgUrl;
            IsLetterRead = isRead;
            if (attending.HasValue) IsAttending = attending;
            if (note != null) GuestNote = note;
        }

        public void MarkLetterAsRead() {
            IsLetterRead = true;
        }

        public void SetLetterRead(bool isRead) {
            IsLetterRead = isRead;
        }

        public void SetAttending(bool? attending) {
            IsAttending = attending;
        }

        public void SetGuestNote(string note) {
            GuestNote = note;
        }

        public void UpdateRSVP(bool attending, string note) {
            IsAttending = attending;
            GuestNote = note;
        }
        #endregion
    }
}