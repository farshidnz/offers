using CashrewardsOffers.Domain.Enums;
using CashrewardsOffers.Domain.Extensions;
using System.Text;

namespace CashrewardsOffers.Domain.Entities
{
    public struct CommissionString
    {
        public string FormattedString { get; }

        public CommissionString(ClientProgramType clientProgramTypeId, CommissionType CommissionType, decimal clientCommission, decimal rate, bool? isFlatRate, RewardType rewardType, string rewardName, bool alwaysUse2DecimalPlaces = false)
        {
            FormattedString = clientProgramTypeId == ClientProgramType.PointsProgram
                ? GetPointsCommissionString(CommissionType, clientCommission, rate, isFlatRate, rewardName)
                : GetCashrewardsCommissionString(CommissionType, clientCommission, rewardType, isFlatRate, rewardName, alwaysUse2DecimalPlaces).Trim();
        }

        private static string GetCashrewardsCommissionString(CommissionType commissionType, decimal clientCommission, RewardType rewardType, bool? isFlatRate, string rewardName, bool alwaysUse2DecimalPlaces)
        {
            var sb = new StringBuilder();
            BuildCashrewardsCommisionString_Pre(sb, rewardType, isFlatRate);

            if (commissionType == CommissionType.Dollar)
            {
                alwaysUse2DecimalPlaces = alwaysUse2DecimalPlaces || clientCommission.RoundToTwoDecimalPlaces() % 1 != 0;
                sb.Append($"${clientCommission.RoundToTwoDecimalPlaces().ToString(alwaysUse2DecimalPlaces ? "F" : "G29")}");
            }
            else
            {
                sb.Append($"{clientCommission.RoundToTwoDecimalPlaces().ToString(alwaysUse2DecimalPlaces ? "F" : "G29")}%");
            }

            BuildCashrewardsCommisionString_Post(sb, rewardType, rewardName);

            return sb.ToString();
        }

        private static string GetPointsCommissionString(CommissionType commissionType, decimal clientCommission, decimal rate, bool? isFlatRate, string rewardName)
        {
            var sb = new StringBuilder();

            var isDollarType = commissionType == CommissionType.Dollar;
            var commissionPts = isDollarType ? clientCommission * rate : clientCommission / 100 * rate;
            commissionPts = commissionPts.RoundToTwoDecimalPlaces();

            if (isFlatRate ?? true)
            {
                sb.Append($"{commissionPts.ToString("G29")} {rewardName}");
            }
            else
            {
                sb.Append($"Up to {commissionPts.ToString("G29")} {rewardName}");
            }

            if (!isDollarType)
            {
                sb.Append("/$");
            }

            return sb.ToString();
        }

        private static void BuildCashrewardsCommisionString_Pre(StringBuilder sb, RewardType rewardType, bool? isFlatRate)
        {
            if (rewardType == RewardType.Discount)
            {
                sb.Append("Save ");
                return;
            }

            if (rewardType == RewardType.MaxDiscount)
            {
                sb.Append("Save up to ");
                return;
            }

            if (isFlatRate ?? false)
            {
                sb.Append(string.Empty);
            }
            else
            {
                sb.Append("Up to ");
            }
        }

        private static void BuildCashrewardsCommisionString_Post(StringBuilder sb, RewardType rewardType, string rewardName)
        {
            if (rewardType == RewardType.Discount)
            {
                return;
            }
            if (rewardType == RewardType.MaxDiscount)
            {
                return;
            }

            sb.Append(" ");
            sb.Append(rewardName);
        }
    }
}
