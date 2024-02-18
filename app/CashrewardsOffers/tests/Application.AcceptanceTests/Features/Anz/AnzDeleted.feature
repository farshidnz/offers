Feature: AnzDeleted

 Handles the case where merchants and/or offers are deleted for any reason. Also when they are reinstated.

Scenario: Deleted merchants should not be returned
	Given merchant data change event
	| MerchantId | PopularMerchantRankingForBrowser | IsPopular | NetworkId |
	| 100        | 1                                | true      | 1000003   |
	| 103        | 2                                | true      | 1000053   |
	When I send an ANZ query
	Then I should receive ANZ items
	| Id  |
	| 100 |
	| 103 |
	Given merchant delete event
	| MerchantId |
	| 100        |
	| 103        |
	When I send an ANZ query
	Then I should receive ANZ items
	| Id |


	
Scenario: Deleted merchants should be returned if reinstated
	Given merchant data change event
	| MerchantId | PopularMerchantRankingForBrowser | IsPopular | NetworkId |
	| 100        | 1                                | true      | 1000003   |
	| 103        | 2                                | true      | 1000053   |
	When I send an ANZ query
	Then I should receive ANZ items
	| Id  |
	| 100 |
	| 103 |
	Given merchant delete event
	| MerchantId |
	| 100        |
	| 103        |
	Given merchant data change event
	| MerchantId | PopularMerchantRankingForBrowser | IsPopular | NetworkId |
	| 100        | 1                                | true      | 1000053   |
	| 103        | 2                                | true      | 1000003   |
	When I send an ANZ query
	Then I should receive ANZ items
	| Id  | Merchant.IsPopular | Merchant.IsInstore |
	| 100 | true               | true               |
	| 103 | true               | false              |



Scenario: Deleted offers should not be returned
	Given offer data change event
	| Merchant.Id | OfferId | IsFeatured |
	| 101         | 301     | true       |
	When I send an ANZ query
	Then I should receive ANZ items
	| Id      |
	| 101-301 |
	Given offer delete event
	| Merchant.Id | OfferId |
	| 101         | 301     |
	When I send an ANZ query
	Then I should receive ANZ items
	| Id      |



Scenario: Deleted offers should be returned if reinstated
	Given offer data change event
	| Merchant.Id | OfferId | IsFeatured |
	| 101         | 301     | true       |
	When I send an ANZ query
	Then I should receive ANZ items
	| Id      |
	| 101-301 |
	Given offer delete event
	| Merchant.Id | OfferId |
	| 101         | 301     |
	Given offer data change event
	| Merchant.Id | OfferId | IsFeatured |
	| 101         | 301     | true       |
	When I send an ANZ query
	Then I should receive ANZ items
	| Id      | Offer.IsFeatured |
	| 101-301 | true             |



Scenario: Unavailable merchants should not be returned
	Given merchant data change event
	| MerchantId | PopularMerchantRankingForBrowser | IsPopular | NetworkId |
	| 101        | 1                                | true      | 1000003   |
	| 102        | 2                                | true      | 1000053   |
	| 103        | 3                                | true      | 1000053   |
	When I send an ANZ query
	Then I should receive ANZ items
	| Id  |
	| 101 |
	| 102 |
	| 103 |
	Given merchant data change event
	| MerchantId | PopularMerchantRankingForBrowser | IsPopular | NetworkId | IsPremiumDisabled | IsPaused | MobileEnabled |
	| 101        | 1                                | true      | 1000003   | true              | false    | true          |
	| 102        | 2                                | true      | 1000053   | false             | true     | true          |
	| 103        | 2                                | true      | 1000053   | false             | false    | false         |
	When I send an ANZ query
	Then I should receive ANZ items
	| Id |



Scenario: Unavailable offers should not be returned
	Given offer data change event
	| Merchant.Id | OfferId | IsFeatured | IsPremiumDisabled | IsPaused | MobileEnabled |
	| 101         | 301     | true       | false             | false    | true          |
	| 101         | 302     | true       | false             | false    | true          |
	| 101         | 303     | true       | false             | false    | true          |
	When I send an ANZ query
	Then I should receive ANZ items
	| Id      |
	| 101-301 |
	| 101-302 |
	| 101-303 |
	Given offer data change event
	| Merchant.Id | OfferId | IsFeatured | Merchant.IsPremiumDisabled | IsMerchantPaused | Merchant.MobileEnabled |
	| 101         | 301     | true       | true                       | false            | true                   |
	| 101         | 302     | true       | false                      | true             | true                   |
	| 101         | 303     | true       | false                      | false            | false                  |
	When I send an ANZ query
	Then I should receive ANZ items
	| Id |
