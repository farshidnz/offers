Feature: AnzInstoreSequence

Ensure in-store carousel items maintain a sequence with no gaps

Scenario: In-store items should be ordered by IsHomePageFeatured then by IsFeatured then by IsPopular then by Name
	Given merchant data change event
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


Scenario: A new in-store item should resequence all below it
	Given merchant data change event
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
	Given merchant data change event
	| MerchantId | IsPopular | NetworkId | IsHomePageFeatured | IsFeatured | Name    |
	| 106        | true      | 1000053   | true               | true       | Sizzler |
	When I send an ANZ query
	Then I should receive ANZ items
	| Merchant.Id | Merchant.IsInstore | Merchant.InstoreRanking |
	| 100         | true               | 7                       |
	| 101         | true               | 6                       |
	| 102         | true               | 3                       |
	| 103         | true               | 2                       |
	| 104         | true               | 5                       |
	| 105         | true               | 4                       |
	| 106         | true               | 1                       |
