Feature: AnzCarouselSelection

An API that gets offers and merchant info for consumption by ANZ bank.

These tests check that ANZ items appear in the correct carousel. Also that they are omitted if not present in any carousel.

Inclusion in carousels is indicated by the following fields in the response.
Merchant.IsPopular - Popular Merchants carousel
Merchant.IsInstore - InStore Merchants carousel
Offer.IsFeatured - Featureed Offers carousel
Offer.IsExclusive - Anz Max carousel


Scenario: Get in-store item
	Given merchant data change event
	| MerchantId | PopularMerchantRankingForBrowser | NetworkId |
	| 103        | 0                                | 1000053   |
	When I send an ANZ query
	Then I should receive ANZ items
	| Id  | Merchant.Id | Merchant.IsPopular | Merchant.IsInstore |
	| 103 | 103         | false              | true               |



Scenario: Get popular item
	Given merchant data change event
	| MerchantId | PopularMerchantRankingForBrowser | IsPopular |
	| 100        | 1                                | true      |
	When I send an ANZ query
	Then I should receive ANZ items
	| Id  | Merchant.Id | Merchant.IsPopular |
	| 100 | 100         | true               |



Scenario: Get featured item
	Given offer data change event
	| OfferId | Merchant.Id | IsFeatured |
	| 301     | 101         | true       |
	When I send an ANZ query
	Then I should receive ANZ items
	| Id      | Merchant.Id | Offer.IsFeatured |
	| 101-301 | 101         | true             |



Scenario: Should not return merchant if PopularMerchantRankingForBrowser changes to zero or IsPopular change to false
	Given merchant data change event
	| MerchantId | PopularMerchantRankingForBrowser | IsPopular |
	| 100        | 1                                | true      |
	| 101        | 2                                | true      |
	When I send an ANZ query
	Then I should receive ANZ items
	| Id      | Merchant.Id | Merchant.IsPopular |
	| 100     | 100         | true               |
	| 101     | 101         | true               |
	Given merchant data change event
	| MerchantId | PopularMerchantRankingForBrowser | IsPopular |
	| 100        | 1                                | false     |
	| 101        | 0                                | true      |
	When I send an ANZ query
	Then I should receive ANZ items
	| Id      |



Scenario: Item should include both IsPopular and IsInstore given the network ID changes
	Given merchant data change event
	| MerchantId | PopularMerchantRankingForBrowser | NetworkId | IsPopular |
	| 100        | 1                                | 1000003   | true      |
	When I send an ANZ query
	Then I should receive ANZ items
	| Id  | Merchant.Id | Merchant.IsPopular | Merchant.IsInstore |
	| 100 | 100         | true               | false              |
	Given merchant data change event
	| MerchantId | PopularMerchantRankingForBrowser | NetworkId |
	| 100        | 1                                | 1000053   |
	When I send an ANZ query
	Then I should receive ANZ items
	| Id  | Merchant.Id | Merchant.IsPopular | Merchant.IsInstore |
	| 100 | 100         | true               | true               |



Scenario: Should not return merchant if IsFeatured changes to false
	Given offer data change event
	| OfferId | Merchant.Id | IsFeatured |
	| 301     | 101         | true       |
	When I send an ANZ query
	Then I should receive ANZ items
	| Id      | Merchant.Id | Offer.IsFeatured |
	| 101-301 | 101         | true             |
	Given offer data change event
	| OfferId | Merchant.Id | IsFeatured |
	| 301     | 101         | false      |
	When I send an ANZ query
	Then I should receive ANZ items
	| Id      |
