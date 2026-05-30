using KYC.Application.Services;
using KYC.Application.Models;
using KYC.Domain.Entities;
using KYC.Domain.Enums;
using KYC.Domain.ValueObjects;

namespace KYC.Application.Tests;

public class UboGraphViewBuilderTests
{
    [Fact]
    public void Merges_case_party_flags_into_graph_nodes_by_nif()
    {
        var kyc = KycCase.Start("508144500", "Acme SA", "u1", CreditAmount.Eur(1000));
        var target = CaseParty.Create(kyc.Id, EntityType.Company, "Acme SA", "508144500", EntityRole.Target, 100, 0, null);
        var ubo = CaseParty.Create(kyc.Id, EntityType.Individual, "João Silva", "123456789", EntityRole.Ubo, 60, 1, target.Id);
        ubo.SetFlags(isPep: true, isSanctioned: false, isOffshore: false, null);
        kyc.AddParty(target);
        kyc.AddParty(ubo);

        var graph = new UboGraph(
        [
            new UboNode(target.Id, "Acme SA", "508144500", "Company", 0, 100m),
            new UboNode(ubo.Id, "João Silva", "123456789", "Individual", 1, 60m)
        ],
        [new UboEdge(ubo.Id, target.Id, 60m)]);

        var view = UboGraphViewBuilder.Build(kyc, graph);
        var joao = view.Nodes.First(n => n.Nif == "123456789");

        Assert.True(joao.IsPep);
        Assert.Equal(ubo.Id, joao.CasePartyId);
        Assert.Equal("UBO", joao.CaseRoleLabel);
    }
}
