namespace VELD.AlterraWeaponry.Mono
{
    public class DepthChargeHandTarget : HandTarget, IHandTarget
    {
        public void OnHandClick(GUIHand hand)
        {
            var behavior = GetComponentInParent<DepthChargeBehavior>();
            if (behavior != null && !behavior.IsPrimed)
            {
                behavior.Prime();
            }
        }

        public void OnHandHover(GUIHand hand)
        {
            var behavior = GetComponentInParent<DepthChargeBehavior>();
            if (behavior != null)
            {
                if (!behavior.IsPrimed)
                {
                    HandReticle.main.SetText(HandReticle.TextType.Hand, "Prime depth charge", true, GameInput.Button.LeftHand);
                    HandReticle.main.SetIcon(HandReticle.IconType.Hand, 1f);
                }
                else
                {
                    HandReticle.main.SetText(HandReticle.TextType.Hand, "Armed", false);
                    HandReticle.main.SetIcon(HandReticle.IconType.HandDeny, 1f);
                }
            }
        }
    }
}