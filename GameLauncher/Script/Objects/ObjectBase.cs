using static GameLauncher.Script.Utils.ObjectUtils;

namespace GameLauncher.Script.Objects;

public abstract class ObjectBase {
  public override string ToString() {
    return ToJsonLikeString(this);
  }
}
