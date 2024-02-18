Feature: ANZ Offers API - Merchants

    As PM
    I want to add the Merchant attributes to the ANZ Offers API
    So that ANZ can store the Merchant attributes and display it in the ANZ App


    Scenario Outline: Available Merchant attribute fields in Offers API for ANZ

        Given API for ANZ is available
        When I call the api
        And auth token is valid
        Then the API returns the '<fields>' for Merchant and Offer in the response as '<mandatory>'

        Examples:
            | fields                                           | mandatory |
            | Items[].Merchant.Id                              | mandatory |
            | Items[].Merchant.Name                            | mandatory |
            | Items[].Merchant.Link                            | mandatory |
            | Items[].Merchant.StartDateTime                   | mandatory |
            | Items[].Merchant.EndDateTime                     | mandatory |
            | Items[].Merchant.LogoUrl                         | mandatory |
            | Items[].Merchant.ClientCommissionString          | mandatory |
            | Items[].Merchant.CashbackGuidelines              | optional  |
            | Items[].Merchant.SpecialTerms                    | optional  |
            | Items[].Merchant.IsInstore                       | mandatory |
            | Items[].Merchant.InstoreTerms                    | optional  |
            | Items[].Merchant.IsPopular                       | mandatory |
            | Items[].Merchant.Categories[].Id                 | mandatory |
            | Items[].Merchant.Categories[].Name               | mandatory |
