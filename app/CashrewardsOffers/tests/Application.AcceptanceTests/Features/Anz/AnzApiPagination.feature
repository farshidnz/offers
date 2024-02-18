Feature: Anz Api Pagination

Ability to ask for a page size and page offset

Scenario: Given no page size or offset return all anz offers
	Given API for ANZ is available
	When auth token is valid
	Given offer data change event
	| OfferId | Merchant.Id | IsFeatured | IsPremiumFeature |
	| 301     | 101         | true       | true             |
	| 302     | 101         | true       | true             |
	When I send an ANZ query
	Then I should receive ANZ items
	| Id      | IsFeatured | IsExculsive |
	| 101-301 | true       | true        |
	| 101-302 | true       | true        |
	Then I should recieve the following response data from the anz api TotalOffersCount = '2', PageOffersCount = '2', TotalPageCount = '1', PageNumber = '1'


	Scenario: Given some page size or offset return anz offers
	Given API for ANZ is available
	When auth token is valid
	Given offer data change event
	| OfferId | Merchant.Id | IsFeatured | IsPremiumFeature |
	| 301     | 101         | true       | true             |
	| 302     | 101         | true       | true             |
	When I send an ANZ query with OffersPerPage = '99999', PageNumber = '1'
	Then I should receive ANZ items
	| Id      | IsFeatured | IsExculsive |
	| 101-301 | true       | true        |
	| 101-302 | true       | true        |
	Then I should recieve the following response data from the anz api TotalOffersCount = '2', PageOffersCount = '2', TotalPageCount = '1', PageNumber = '1'
	When I send an ANZ query with OffersPerPage = '1', PageNumber = '1'
	Then I should receive ANZ items
	| Id      | IsFeatured | IsExculsive |
	| 101-301 | true       | true        |
	Then I should recieve the following response data from the anz api TotalOffersCount = '2', PageOffersCount = '1', TotalPageCount = '2', PageNumber = '1'
	When I send an ANZ query with OffersPerPage = '1', PageNumber = '2'
	Then I should receive ANZ items
	| Id      | IsFeatured | IsExculsive |
	| 101-302 | true       | true        |
	Then I should recieve the following response data from the anz api TotalOffersCount = '2', PageOffersCount = '1', TotalPageCount = '2', PageNumber = '2'
	Given offer data change event
	| OfferId | Merchant.Id | IsFeatured | IsPremiumFeature |
	| 303     | 102         | true       | true             |
	| 304     | 103         | true       | true             |
	| 305     | 104         | true       | true             |
	| 306     | 105         | true       | true             |
	| 307     | 106         | true       | true             |
	When I send an ANZ query with OffersPerPage = '5', PageNumber = '1'
	Then I should receive ANZ items
	| Id      | IsFeatured | IsExculsive |
	| 101-301 | true       | true        |
	| 101-302 | true       | true        |
	| 102-303 | true       | true        |
	| 103-304 | true       | true        |
	| 104-305 | true       | true        |
	Then I should recieve the following response data from the anz api TotalOffersCount = '7', PageOffersCount = '5', TotalPageCount = '2', PageNumber = '1'
	When I send an ANZ query with OffersPerPage = '5', PageNumber = '2'
	Then I should receive ANZ items
	| Id      | IsFeatured | IsExculsive |
	| 105-306 | true       | true        |
	| 106-307 | true       | true        |
	Then I should recieve the following response data from the anz api TotalOffersCount = '7', PageOffersCount = '2', TotalPageCount = '2', PageNumber = '2'
	Given offer delete event
	| OfferId | Merchant.Id |
	| 301     | 101         |
	| 302     | 101         |
	When I send an ANZ query with OffersPerPage = '3', PageNumber = '1'
	Then I should receive ANZ items
	| Id      | IsFeatured | IsExculsive |
	| 102-303 | true       | true        |
	| 103-304 | true       | true        |
	| 104-305 | true       | true        |
	Then I should recieve the following response data from the anz api TotalOffersCount = '5', PageOffersCount = '3', TotalPageCount = '2', PageNumber = '1'
	When I send an ANZ query with OffersPerPage = '3', PageNumber = '2'
	Then I should receive ANZ items
	| Id      | IsFeatured | IsExculsive |
	| 105-306 | true       | true        |
	| 106-307 | true       | true        |
	Then I should recieve the following response data from the anz api TotalOffersCount = '5', PageOffersCount = '2', TotalPageCount = '2', PageNumber = '2'
