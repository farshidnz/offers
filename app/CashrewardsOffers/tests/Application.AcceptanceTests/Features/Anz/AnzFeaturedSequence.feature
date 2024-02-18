Feature: AnzFeaturedSequence

Scenario: Featured offer items should be ordered by rank then first ending
	Given offer data change event
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

Scenario: Only Featured offers should update when events come through
	Given offer data change event
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
	Given offer data change event
	| Merchant.Id | EndDateTime        | IsFeatured | OfferId | Ranking |
	| 106         | 637922304380000000 | true       | 106     | 1       |
	When I send an ANZ query
	Then I should receive ANZ items
	| ItemId  | Offer.IsFeatured | Offer.FeaturedRanking |
	| 100-100 | true             | 4                     |
	| 101-101 | true             | 5                     |
	| 102-102 | true             | 6                     |
	| 103-103 | true             | 2                     |
	| 104-104 | true             | 7                     |
	| 105-105 | true             | 1                     |
	| 106-106 | true             | 3                     |
	Given offer data change event
	| Merchant.Id | EndDateTime        | IsFeatured | OfferId | Ranking |
	| 103         | 637922304380000000 | false      | 103     | 1       |
	When I send an ANZ query
	Then I should receive ANZ items
	| ItemId  | Offer.IsFeatured | Offer.FeaturedRanking |
	| 100-100 | true             | 3                     |
	| 101-101 | true             | 4                     |
	| 102-102 | true             | 5                     |
	| 104-104 | true             | 6                     |
	| 105-105 | true             | 1                     |
	| 106-106 | true             | 2                     |
	Given offer data change event
	| Merchant.Id |
	| 102         |
	When I send an ANZ query
	Then I should receive ANZ items
	| ItemId  | Offer.IsFeatured | Offer.FeaturedRanking |
	| 100-100 | true             | 3                     |
	| 101-101 | true             | 4                     |
	| 102-102 | true             | 5                     |
	| 104-104 | true             | 6                     |
	| 105-105 | true             | 1                     |
	| 106-106 | true             | 2                     |
	Given offer data change event
	| Merchant.Id | EndDateTime        |
	| 102         | 627922304380000000 |
	When I send an ANZ query
	Then I should receive ANZ items
	| ItemId  | Offer.IsFeatured | Offer.FeaturedRanking |
	| 100-100 | true             | 4                     |
	| 101-101 | true             | 5                     |
	| 102-102 | true             | 3                     |
	| 104-104 | true             | 6                     |
	| 105-105 | true             | 1                     |
	| 106-106 | true             | 2                     |
	Given offer delete event
	| OfferId | Merchant.Id |
	| 106     | 106         |
	When I send an ANZ query
	Then I should receive ANZ items
	| ItemId  | Offer.IsFeatured | Offer.FeaturedRanking |
	| 100-100 | true             | 3                     |
	| 101-101 | true             | 4                     |
	| 102-102 | true             | 2                     |
	| 104-104 | true             | 5                     |
	| 105-105 | true             | 1                     |
	Given offer data change event
	| Merchant.Id | EndDateTime        | IsFeatured | OfferId | Ranking |
	| 106         | 637922304380000000 | true       | 106     | 1       |
	When I send an ANZ query
	Then I should receive ANZ items
	| ItemId  | Offer.IsFeatured | Offer.FeaturedRanking |
	| 100-100 | true             | 4                     |
	| 101-101 | true             | 5                     |
	| 102-102 | true             | 3                     |
	| 104-104 | true             | 6                     |
	| 105-105 | true             | 1                     |
	| 106-106 | true             | 2                     |
