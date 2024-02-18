Feature: AnzDelta

 Allows retrieval of ANZ items which have changed after a certiain point in time.

Scenario: Should only get merchant changes made after a point in time
	Given merchant data change event at '30/07/2022 12:00:00'
	| MerchantId | PopularMerchantRankingForBrowser | IsPopular | NetworkId |
	| 100        | 1                                | true      | 1000003   |
	| 101        | 2                                | true      | 1000053   |
	Given merchant data change event at '30/07/2022 13:00:00'
	| MerchantId | PopularMerchantRankingForBrowser | IsPopular | NetworkId |
	| 103        | 3                                | true      | 1000053   |
	When I send an ANZ query with update after '30/07/2022 12:01:00'
	Then I should receive ANZ items
	| Id  | PopularMerchantRankingForBrowser |
	| 103 | 3                                |



Scenario: Should only get offer changes made after a point in time
	Given offer data change event at '30/07/2022 12:00:00'
	| Merchant.Id | OfferId | IsFeatured | Ranking |
	| 101         | 301     | true       | 1       |
	| 101         | 302     | true       | 2       |
	Given offer data change event at '30/07/2022 13:00:00'
	| Merchant.Id | OfferId | IsFeatured | Ranking |
	| 101         | 302     | true       | 5       |
	When I send an ANZ query with update after '30/07/2022 12:01:00'
	Then I should receive ANZ items
	| Id      | FeaturedRanking |
	| 101-302 | 5               |


	
Scenario: Should only get merchant deletions made after a point in time
	Given merchant data change event at '30/07/2022 12:00:00'
	| MerchantId | PopularMerchantRankingForBrowser | IsPopular | NetworkId |
	| 100        | 1                                | true      | 1000003   |
	| 101        | 2                                | true      | 1000053   |
	Given merchant delete event at '30/07/2022 12:30:00'
	| MerchantId |
	| 101        |
	Given merchant delete event at '30/07/2022 13:00:00'
	| MerchantId |
	| 100        |
	When I send an ANZ query with update after '30/07/2022 12:31:00'
	Then I should receive ANZ items
	| Id  | IsDeleted |
	| 100 | true      |



Scenario: Should only get offer deletions made after a point in time
	Given offer data change event at '30/07/2022 12:00:00'
	| Merchant.Id | OfferId | IsFeatured |
	| 101         | 301     | true       |
	| 101         | 302     | true       |
	Given offer delete event at '30/07/2022 12:30:00'
	| Merchant.Id | OfferId |
	| 101         | 302     |
	Given offer delete event at '30/07/2022 13:00:00'
	| Merchant.Id | OfferId |
	| 101         | 301     |
	When I send an ANZ query with update after '30/07/2022 12:31:00'
	Then I should receive ANZ items
	| Id      | IsDeleted |
	| 101-301 | true      |



Scenario: Unavailable merchants should be returned as deleted
	Given merchant data change event at '30/07/2022 12:00:00'
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
	Given merchant data change event at '30/07/2022 12:30:00'
	| MerchantId | PopularMerchantRankingForBrowser | IsPopular | NetworkId | IsPremiumDisabled | IsPaused | MobileEnabled |
	| 101        | 1                                | true      | 1000003   | true              | false    | true          |
	| 102        | 2                                | true      | 1000053   | false             | true     | true          |
	| 103        | 2                                | true      | 1000053   | false             | false    | false         |
	When I send an ANZ query with update after '30/07/2022 12:29:00'
	Then I should receive ANZ items
	| Id  | IsDeleted |
	| 101 | true      |
	| 102 | true      |
	| 103 | true      |


	
Scenario: Unavailable merchants should be returned as deleted after a point in time
	Given merchant data change event at '30/07/2022 12:00:00'
	| MerchantId | PopularMerchantRankingForBrowser | IsPopular | NetworkId | IsPremiumDisabled | IsPaused | MobileEnabled |
	| 101        | 1                                | true      | 1000003   | false             | false    | true          |
	| 102        | 2                                | true      | 1000053   | false             | false    | true          |
	| 103        | 3                                | true      | 1000053   | false             | false    | true          |
	| 104        | 4                                | true      | 1000053   | false             | false    | true          |
	| 105        | 5                                | true      | 1000053   | false             | false    | true          |
	| 106        | 6                                | true      | 1000053   | false             | false    | true          |
	| 107        | 7                                | true      | 1000053   | false             | false    | true          |
	| 108        | 8                                | true      | 1000053   | false             | false    | true          |
	Given merchant data change event at '30/07/2022 12:30:00'
	| MerchantId | PopularMerchantRankingForBrowser | IsPopular | NetworkId | IsPremiumDisabled | IsPaused | MobileEnabled |
	| 102        | 0                                | false     | 1000003   | false             | false    | true          |
	| 104        | 3                                | true      | 1000003   | true              | false    | true          |
	| 106        | 6                                | true      | 1000053   | false             | true     | true          |
	| 108        | 8                                | true      | 1000053   | false             | false    | false         |
	Given merchant data change event at '30/07/2022 13:00:00'
	| MerchantId | PopularMerchantRankingForBrowser | IsPopular | NetworkId | IsPremiumDisabled | IsPaused | MobileEnabled |
	| 101        | 0                                | false     | 1000003   | false             | false    | true          |
	| 103        | 4                                | true      | 1000003   | true              | false    | true          |
	| 105        | 5                                | true      | 1000053   | false             | true     | true          |
	| 107        | 7                                | true      | 1000053   | false             | false    | false         |
	When I send an ANZ query with update after '30/07/2022 12:59:00'
	Then I should receive ANZ items
	| Id  | Merchant.IsPopular | IsDeleted | Merchant.PopularRanking |
	| 101 | false              | true      | 0                       |
	| 103 | true               | true      | 1                       |
	| 105 | true               | true      | 1                       |
	| 107 | true               | true      | 1                       |

	

Scenario: Unavailable offers should be returned as deleted
	Given offer data change event at '30/07/2022 12:00:00'
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
	Given offer data change event at '30/07/2022 12:30:00'
	| Merchant.Id | OfferId | IsFeatured | Merchant.IsPremiumDisabled | IsMerchantPaused | Merchant.MobileEnabled |
	| 101         | 301     | true       | true                       | false            | true                   |
	| 101         | 302     | true       | false                      | true             | true                   |
	| 101         | 303     | true       | false                      | false            | false                  |
	When I send an ANZ query with update after '30/07/2022 12:29:00'
	Then I should receive ANZ items
	| Id      | IsDeleted |
	| 101-301 | true      |
	| 101-302 | true      |
	| 101-303 | true      |
	


Scenario: Unavailable offers should be returned as deleted after a point in time
	Given offer data change event at '30/07/2022 12:00:00'
	| Merchant.Id | OfferId | Ranking | IsFeatured | Merchant.IsPremiumDisabled | IsMerchantPaused | Merchant.MobileEnabled |
	| 101         | 301     | 9       | true       | false                      | false            | true                   |
	| 101         | 302     | 8       | true       | false                      | false            | true                   |
	| 101         | 303     | 7       | true       | false                      | false            | true                   |
	| 101         | 304     | 6       | true       | false                      | false            | true                   |
	| 101         | 305     | 5       | true       | false                      | false            | true                   |
	| 101         | 306     | 4       | true       | false                      | false            | true                   |
	| 101         | 307     | 3       | true       | false                      | false            | true                   |
	| 101         | 308     | 2       | true       | false                      | false            | true                   |
	When I send an ANZ query
	Then I should receive ANZ items
	| Id      | Offer.IsFeatured | IsDeleted | Offer.FeaturedRanking |
	| 101-301 | true             | false     | 1                     |
	| 101-302 | true             | false     | 2                     |
	| 101-303 | true             | false     | 3                     |
	| 101-304 | true             | false     | 4                     |
	| 101-305 | true             | false     | 5                     |
	| 101-306 | true             | false     | 6                     |
	| 101-307 | true             | false     | 7                     |
	| 101-308 | true             | false     | 8                     |
	Given offer data change event at '30/07/2022 12:30:00'
	| Merchant.Id | OfferId | IsFeatured | Merchant.IsPremiumDisabled | IsMerchantPaused | Merchant.MobileEnabled |
	| 101         | 302     | false      | false                      | false            | true                   |
	| 101         | 304     | true       | true                       | false            | true                   |
	| 101         | 306     | true       | false                      | true             | true                   |
	| 101         | 308     | true       | false                      | false            | false                  |
	Given offer data change event at '30/07/2022 13:00:00'
	| Merchant.Id | OfferId | IsFeatured | Merchant.IsPremiumDisabled | IsMerchantPaused | Merchant.MobileEnabled |
	| 101         | 301     | false      | false                      | false            | true                   |
	| 101         | 303     | true       | true                       | false            | true                   |
	| 101         | 305     | true       | false                      | true             | true                   |
	| 101         | 307     | true       | false                      | false            | false                  |
	When I send an ANZ query with update after '30/07/2022 12:31:00'
	Then I should receive ANZ items
	| Id      | Offer.IsFeatured | IsDeleted | Offer.FeaturedRanking |
	| 101-301 | false            | true      | 0                     |
	| 101-303 | true             | true      | 1                     |
	| 101-305 | true             | true      | 1                     |
	| 101-307 | true             | true      | 1                     |



Scenario: Should get all merchants that were resequenced due to popular ranking change after a point in time
	Given merchant data change event at '30/07/2022 12:00:00'
	| MerchantId | PopularMerchantRankingForBrowser | IsPopular |
	| 100        | 1                                | true      |
	| 101        | 2                                | true      |
	| 102        | 4                                | true      |
	| 103        | 5                                | true      |
	| 104        | 6                                | true      |
	| 105        | 7                                | true      |
	When I send an ANZ query
	Then I should receive ANZ items
	| Id  | Merchant.IsPopular | Merchant.PopularRanking |
	| 100 | true               | 1                       |
	| 101 | true               | 2                       |
	| 102 | true               | 3                       |
	| 103 | true               | 4                       |
	| 104 | true               | 5                       |
	| 105 | true               | 6                       |
	Given merchant data change event at '30/07/2022 12:30:00'
	| MerchantId | PopularMerchantRankingForBrowser | IsPopular |
	| 105        | 3                                | true      |
	When I send an ANZ query with update after '30/07/2022 12:29:00'
	Then I should receive ANZ items
	| Id  | Merchant.IsPopular | Merchant.PopularRanking |
	| 102 | true               | 4                       |
	| 103 | true               | 5                       |
	| 104 | true               | 6                       |
	| 105 | true               | 3                       |



Scenario: Should get all merchants that were resequenced due to in-store ranking change after a point in time
	Given merchant data change event at '30/07/2022 12:00:00'
	| MerchantId | IsPopular | NetworkId | IsHomePageFeatured | IsFeatured | Name         |
	| 100        | true      | 1000053   | false              | false      | Pizza Hut    |
	| 101        | true      | 1000053   | false              | false      | MacDonalds   |
	| 102        | true      | 1000053   | true               | false      | Nazimi       |
	| 103        | true      | 1000053   | true               | false      | KFC          |
	| 104        | true      | 1000053   | false              | true       | Red Rooster  |
	| 105        | true      | 1000053   | false              | true       | New Shanghai |
	When I send an ANZ query
	Then I should receive ANZ items
	| Merchant.Id | Merchant.IsInstore | Merchant.InstoreRanking |
	| 100         | true               | 6                       |
	| 101         | true               | 5                       |
	| 102         | true               | 2                       |
	| 103         | true               | 1                       |
	| 104         | true               | 4                       |
	| 105         | true               | 3                       |
	Given merchant data change event at '30/07/2022 12:30:00'
	| MerchantId | IsPopular | NetworkId | IsHomePageFeatured | IsFeatured | Name    |
	| 106        | true      | 1000053   | false              | true       | Sizzler |
	When I send an ANZ query with update after '30/07/2022 12:29:00'
	Then I should receive ANZ items
	| Merchant.Id | Merchant.IsInstore | Merchant.InstoreRanking |
	| 100         | true               | 7                       |
	| 101         | true               | 6                       |
	| 106         | true               | 5                       |



Scenario: Should get all offers that were resequenced due to featured offer ranking change after a point in time
	Given offer data change event at '30/07/2022 12:00:00'
	| Merchant.Id | EndDateTime        | IsFeatured | OfferId | Ranking |
	| 100         | 637922304380000000 | true       | 100     | 0       |
	| 101         | 637922304380000000 | true       | 101     | 0       |
	| 102         | 637922304380000000 | true       | 102     | 0       |
	| 103         | 637922304380000000 | true       | 103     | 1       |
	| 104         | 637922304380000000 | true       | 104     | 0       |
	| 105         | 637922304380000000 | true       | 105     | 999     |
	When I send an ANZ query
	Then I should receive ANZ items
	| ItemId  | Offer.IsFeatured | Offer.FeaturedRanking |
	| 100-100 | true             | 3                     |
	| 101-101 | true             | 4                     |
	| 102-102 | true             | 5                     |
	| 103-103 | true             | 2                     |
	| 104-104 | true             | 6                     |
	| 105-105 | true             | 1                     |
	Given offer data change event at '30/07/2022 12:30:00'
	| Merchant.Id | EndDateTime        | IsFeatured | OfferId | Ranking |
	| 101         | 637922304380000000 | true       | 101     | 3       |
	When I send an ANZ query with update after '30/07/2022 12:29:00'
	Then I should receive ANZ items
	| ItemId  | Offer.IsFeatured | Offer.FeaturedRanking |
	| 100-100 | true             | 4                     |
	| 101-101 | true             | 3                     |
	| 103-103 | true             | 2                     |
