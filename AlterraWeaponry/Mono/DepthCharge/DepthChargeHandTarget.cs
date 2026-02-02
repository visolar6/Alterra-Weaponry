namespace VELD.AlterraWeaponry.Mono.DepthCharge
{
    public class DepthChargeHandTarget : HandTarget, IHandTarget
    {
        public void OnHandClick(GUIHand hand)
        {
            var behavior = GetComponentInParent<DepthChargeBehavior>();
            if (behavior != null)
            {
                if (behavior.IsPrimed)
                {
                    behavior.Disarm();
                }
                else if (!behavior.IsPriming)
                {
                    behavior.Prime();
                }
            }
        }

        public void OnHandHover(GUIHand hand)
        {
            var behavior = GetComponentInParent<DepthChargeBehavior>();
            if (behavior != null)
            {
                if (behavior.IsDetonated)
                    return;

                if (behavior.IsPrimed)
                {
                    HandReticle.main.SetText(HandReticle.TextType.Hand, "DepthCharge_Disarm", true, GameInput.Button.LeftHand);
                    HandReticle.main.SetIcon(HandReticle.IconType.Hand, 1f);
                    return;
                }
                else if (behavior.IsPriming)
                {
                    HandReticle.main.SetText(HandReticle.TextType.Hand, "DepthCharge_Arming", true);
                    HandReticle.main.SetIcon(HandReticle.IconType.HandDeny, 1f);
                    return;
                }
                else
                {
                    HandReticle.main.SetText(HandReticle.TextType.Hand, "DepthCharge_Arm", true, GameInput.Button.LeftHand);
                    HandReticle.main.SetIcon(HandReticle.IconType.Hand, 1f);
                    return;
                }
            }
        }
    }
}