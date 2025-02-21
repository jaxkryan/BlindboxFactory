namespace Script.Controller {
    public class ResourceController : ControllerBase {
        public override void Load() { throw new System.NotImplementedException(); }
        public override void Save() { throw new System.NotImplementedException(); }

        public enum ResourceType {
            Gold,
            Diamond,
            Cloud,
            Rainbow,
            Marshmallow,
            Ruby,
            Star,
        }
    }
}