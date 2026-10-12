"""Read-only validation of the conditional BMAD sprint plan (PyYAML 6.0.3)."""

from copy import deepcopy
from datetime import datetime
from pathlib import Path
import re
import sys
from urllib.parse import unquote

try:
    import yaml
except ImportError:
    sys.exit("Nedostaje PyYAML 6.0.3; koristite izolovano Python okruženje za dokumentacione provjere.")

ROOT = Path(__file__).resolve().parents[2]


class UniqueKeyLoader(yaml.SafeLoader):
    """Reject duplicate mapping keys instead of silently accepting the last one."""


def construct_mapping(loader, node, deep=False):
    result = {}
    for key_node, value_node in node.value:
        key = loader.construct_object(key_node, deep=deep)
        if key in result:
            raise ValueError(f"Duplirani YAML ključ: {key}")
        result[key] = loader.construct_object(value_node, deep=deep)
    return result


UniqueKeyLoader.add_constructor(
    yaml.resolver.BaseResolver.DEFAULT_MAPPING_TAG, construct_mapping
)


def require(condition, message):
    if not condition:
        raise ValueError(message)


def read_yaml(path):
    data = yaml.load(path.read_text(encoding="utf-8-sig"), Loader=UniqueKeyLoader)
    require(isinstance(data, dict), f"YAML nije mapa: {path}")
    return data


def validate_authorization(state, workflow):
    implementation = state["implementation"]
    authorized = implementation["authorized"]
    active_phase = implementation["active_phase"]
    require(type(authorized) is bool and type(workflow["implementation_authorized"]) is bool, "Autorizacija mora biti boolean")
    require(workflow["implementation_authorized"] == authorized and workflow["implementation_phase"] == active_phase, "Autorizacija faze nije usklađena između evidencija")
    require(type(implementation["started"]) is bool, "Početak implementacije mora biti boolean")
    require(not implementation["started"] or authorized, "Razvoj nije dozvoljen prije aktivacije")
    conditions = {entry["id"]: entry for entry in state["entry_conditions"]}
    require("PHASE-01-AUTHORIZATION" in conditions, "Nedostaje uslov autorizacije")
    if authorized:
        require(active_phase == "01", "Odobrena je isključivo Faza 01")
        require(conditions["PHASE-01-AUTHORIZATION"]["status"] == "satisfied", "Aktivirana faza nema ispunjen uslov autorizacije")
        timestamp = implementation.get("authorized_at")
        require(isinstance(timestamp, str), "Nedostaje vrijeme autorizacije")
        require(datetime.fromisoformat(timestamp.replace("Z", "+00:00")).tzinfo is not None, "Vrijeme autorizacije nema vremensku zonu")
        request = implementation.get("authorization_request")
        require(isinstance(request, str) and request.strip(), "Nedostaje zapis zahtjeva vlasnika")
    else:
        require(active_phase is None, "Neodobrena faza ne može biti aktivna")
        require(conditions["PHASE-01-AUTHORIZATION"]["status"] == "pending", "Neodobrena faza ima ispunjen uslov autorizacije")
    require(conditions["CAPACITY-CONFIRMATION"]["status"] == "pending" and conditions["BOOTSTRAP-GATE"]["status"] == "pending", "Aktivacija ne potvrđuje kapacitet ili bootstrap gate")


def validate_state(state, workflow):
    stories = state["stories"]
    ids = [story["id"] for story in stories]
    require(len(ids) == len(set(ids)), "Duplirani ID priče")
    require(set(ids) == {f"STORY-{number:03}" for number in range(24, 31)}, "Nedostaje razložena priča")
    by_id = {story["id"]: story for story in stories}
    for story in stories:
        require(story["points"] in (1, 2, 3, 5, 8), "Procjena nije dozvoljena Fibonacci vrijednost")
        expected_status = "in_progress" if state["implementation"]["started"] and story["id"] == "STORY-024" else "not_started"
        require(story["status"] == expected_status, "Status priče nije usklađen sa početkom STORY-024")
        require(story["priority"] == "MUST" and story["epic"] == "EPIC-001", "Pogrešan opseg/prioritet")
        require(set(story["dependencies"]) <= set(ids), "Nepoznata zavisnost priče")
        require(story["parent_ids"] and all(1 <= int(parent.split("-")[1]) <= 23 for parent in story["parent_ids"]), "Nepoznat PRD roditelj")

    visiting, visited = set(), set()

    def visit(story_id):
        require(story_id not in visiting, "Ciklus zavisnosti")
        if story_id in visited:
            return
        visiting.add(story_id)
        for dependency in by_id[story_id]["dependencies"]:
            visit(dependency)
        visiting.remove(story_id)
        visited.add(story_id)

    for story_id in ids:
        visit(story_id)

    sprint = state["sprint"]
    selected = sprint["story_ids"]
    backlog = state["backlog_story_ids"]
    require(len(selected) == len(set(selected)) and len(backlog) == len(set(backlog)), "Duplirana stavka sprinta")
    require(set(selected).isdisjoint(backlog) and set(selected + backlog) == set(ids), "Sprint i preostali obim nisu potpuna podjela")
    for story_id in selected:
        require(set(by_id[story_id]["dependencies"]) <= set(selected), "Sprint zavisi od neplanirane priče")
    for story in stories:
        expected_sprint = 1 if story["id"] in selected else None
        require(story["sprint_planned"] == expected_sprint, "Neslaganje rasporeda priče")

    total = sum(story["points"] for story in stories)
    proposed = sum(by_id[story_id]["points"] for story_id in selected)
    metrics = state["metrics"]
    for key, expected in {
        "total_stories": len(stories), "proposed_sprint_stories": len(selected),
        "backlog_stories": len(backlog), "total_points": total,
        "proposed_sprint_points": proposed, "backlog_points": total - proposed,
        "remaining_points": total,
    }.items():
        require(metrics[key] == expected, f"Netačan zbir: {key}")
    require(all(metrics[key] == 0 for key in ("completed_stories", "blocked_stories", "completed_points", "completion_percentage")), "Izmišljeni rezultat razvoja")
    require(metrics["in_progress_stories"] == int(state["implementation"]["started"]), "Netačan broj priča u toku")
    require(sprint["proposed_points"] == proposed and sprint["committed_points"] == 0, "Netačan uslovni obim")
    require(sprint["status"] == "not_started" and sprint["planning_status"] == "conditional", "Pogrešan status sprinta")
    require(sprint["start_date"] is None and sprint["end_date"] is None and sprint["capacity_points"] is None, "Izmišljen rok/kapacitet")
    require(not state["capacity"]["confirmed"], "Kapacitet uslovnog plana je nepotvrđen")
    scenario = state["capacity"]["scenario"]
    require(scenario["base_points"] == scenario["development_flows"] * scenario["available_dev_days"] * scenario["points_per_dev_day"], "Netačan bazni scenario")
    require(scenario["adjusted_points"] == scenario["base_points"] * (1 - scenario["reserve_percentage"] / 100), "Netačna rezerva")
    require(proposed <= scenario["adjusted_points"], "Predloženi obim prelazi scenario kapaciteta")
    validate_authorization(state, workflow)
    phase = workflow["phases"]["phase_4_implementation"]["workflows"]
    require(phase["sprint_planning"]["completed"] and phase["sprint_planning"]["planning_status"] == "conditional", "Plan nije evidentiran")
    require(set(phase["stories"]["story_list"]) == set(ids) and phase["stories"]["total"] == len(stories), "Workflow priče nisu usklađene")
    require(phase["stories"]["completed"] == 0 and phase["stories"]["in_progress"] == metrics["in_progress_stories"], "Workflow nije usklađen sa napretkom priča")
    gate = workflow["phases"]["phase_3_solutioning"]["workflows"]["gate_check"]
    require(gate["decision"] == "CONDITIONAL PASS" and gate["passed"] is False, "Gate je neosnovano unaprijeđen")
    completed = sum(item.get("completed") is True for value in workflow["phases"].values() for item in value["workflows"].values())
    require(workflow["metrics"]["completed_workflows"] == completed, "Netačan broj završenih tokova")
    require(workflow["metrics"]["progress_percentage"] == completed / workflow["metrics"]["total_workflows"] * 100, "Netačan BMAD napredak")
    require(workflow["metrics"]["estimated_stories"] == len(stories) and workflow["metrics"]["completed_stories"] == 0, "Netačna workflow metrika priča")
    require(state["velocity"]["history"] == [] and state["velocity"]["rolling_average"] is None and state["burndown"] == [], "Izmišljena istorija rada")
    require(any(risk["id"] == "G-001" and risk["status"] == "open" for risk in state["risks"]), "G-001 je neosnovano zatvoren")
    require({entry["id"] for entry in state["entry_conditions"]} >= {"PHASE-01-AUTHORIZATION", "CAPACITY-CONFIRMATION", "BOOTSTRAP-GATE"}, "Nedostaje uslov ulaska")


def validate_documents(state, workflow):
    paths = [ROOT / "docs/bmad/sprint-plan.md", ROOT / "README.md", ROOT / "docs/bmad/architecture.md"]
    paths += [ROOT / story["output_file"] for story in state["stories"]]
    project = read_yaml(ROOT / "bmad/project.yaml")
    require(all("Republici Srpskoj" in value for value in project["language"].values()), "Nije podešen jezik vlasnika")
    for phase in workflow["phases"].values():
        for item in phase["workflows"].values():
            if item.get("output_file"):
                require((ROOT / item["output_file"]).is_file(), "Nedostaje workflow artefakt")
    for story in state["stories"]:
        path = ROOT / story["output_file"]
        require(path.is_file(), f"Nedostaje fajl: {path}")
        content = path.read_text(encoding="utf-8-sig")
        require(content.startswith(f"# {story['id']} — {story['title']}"), "Naslov priče nije usklađen")
        criteria = re.findall(r"^- \[ \] AC-(\d{2}):", content, re.MULTILINE)
        require(3 <= len(criteria) <= 7 and len(criteria) == len(set(criteria)), "Priča nema 3–7 jedinstvenih kriterijuma")
        require(f"**Procjena:** {story['points']} bod" in content and f"**Status:** {story['status']}" in content, "Procjena/status dokumenta nije usklađen")
        for section in ("Korisnička priča", "Kriterijumi prihvatanja", "Implementacija i granice", "Zavisnosti i rubni slučajevi", "Provjere i završetak"):
            require(f"## {section}" in content, f"Nedostaje sekcija: {section}")
        for parent in story["parent_ids"]:
            require(parent in content, "Nedostaje roditeljska sljedivost")
    plan = paths[0].read_text(encoding="utf-8-sig")
    coverage = re.findall(r"^\| (F01-AC-\d{2}):", plan, re.MULTILINE)
    require(set(coverage) == {f"F01-AC-{number:02}" for number in range(1, 7)} and len(coverage) == 6, "Acceptance matrica nije potpuna")
    link_count = 0
    for path in paths:
        content = path.read_text(encoding="utf-8-sig")
        for target in re.findall(r"\[[^\]]+\]\(([^)]+)\)", content):
            if re.match(r"[a-z]+://", target) or target.startswith("#"):
                continue
            local = unquote(target.split("#", 1)[0]).strip("<>")
            require((path.parent / local).exists(), f"Neispravna lokalna veza: {path.name}: {local}")
            link_count += 1
    for path in [paths[0]] + paths[3:]:
        content = path.read_text(encoding="utf-8-sig")
        require(not re.search(r"\b(nijesam|nijesi|nijesmo|nijeste|nijesu|sjutra)\b", content, re.IGNORECASE), "Nedozvoljen jezički oblik")
    return link_count


def self_test(state, workflow):
    # Both supported states must pass; authorization is independent of sprint start.
    for authorized, started in ((False, False), (True, False), (True, True)):
        control_state, control_workflow = deepcopy(state), deepcopy(workflow)
        phase = "01" if authorized else None
        control_state["implementation"].update(active_phase=phase, authorized=authorized, started=started)
        control_state["implementation"].update(authorized_at="2026-10-12T05:42:08Z", authorization_request="Izričito aktiviranje Faze 01")
        control_workflow.update(implementation_phase=phase, implementation_authorized=authorized)
        control_state["stories"][0]["status"] = "in_progress" if started else "not_started"
        control_state["metrics"]["in_progress_stories"] = int(started)
        control_workflow["phases"]["phase_4_implementation"]["workflows"]["stories"]["in_progress"] = int(started)
        next(entry for entry in control_state["entry_conditions"] if entry["id"] == "PHASE-01-AUTHORIZATION")["status"] = "satisfied" if authorized else "pending"
        validate_state(control_state, control_workflow)
    print("Pozitivne kontrole: 3/3; plan bez aktivacije, aktivacija i početak STORY-024.")
    mutations = [
        lambda item: item["metrics"].update(total_points=0),
        lambda item: item["stories"][0]["dependencies"].append("STORY-025"),
        lambda item: item["stories"][0]["dependencies"].append("STORY-999"),
        lambda item: item["implementation"].update(authorized=not item["implementation"]["authorized"]),
        lambda item: item["sprint"].update(start_date="2026-10-12"),
        lambda item: item["stories"].append(deepcopy(item["stories"][0])),
        lambda item: item["implementation"].update(active_phase="02"),
        lambda item: item["implementation"].update(started=not item["implementation"]["started"]),
    ]
    for mutate in mutations:
        changed = deepcopy(state)
        mutate(changed)
        try:
            validate_state(changed, workflow)
        except ValueError:
            continue
        raise ValueError("Negativna kontrola nije odbijena")
    try:
        yaml.load("name: first\nname: second\n", Loader=UniqueKeyLoader)
    except ValueError:
        pass
    else:
        raise ValueError("Duplirani YAML ključ nije odbijen")
    # Check evidence requirements even when validating a historical inactive plan.
    for key in ("authorized_at", "authorization_request"):
        changed = deepcopy(control_state)
        changed["implementation"].pop(key)
        try:
            validate_state(changed, control_workflow)
        except ValueError:
            continue
        raise ValueError("Aktivacija bez evidencije zahtjeva nije odbijena")
    count = len(mutations) + 3
    print(f"Negativne kontrole: {count}/{count} odbijeno; fajlovi nisu mijenjani.")


def main():
    state = read_yaml(ROOT / "bmad/sprint-status.yaml")
    workflow = read_yaml(ROOT / "bmad/workflow-status.yaml")
    validate_state(state, workflow)
    links = validate_documents(state, workflow)
    metrics = state["metrics"]
    print(f"Plan validan: {metrics['total_stories']} priča, {metrics['total_points']} boda; sprint 1: {metrics['proposed_sprint_stories']}/{metrics['proposed_sprint_points']}, preostalo: {metrics['backlog_stories']}/{metrics['backlog_points']}; 6 acceptance redova; {links} lokalnih veza.")
    status = "Faza 01 aktivirana" if state["implementation"]["authorized"] else "implementacija nije aktivirana"
    progress = "STORY-024 je u toku" if state["implementation"]["started"] else "priče nisu započete"
    print(f"YAML, zavisnosti i jezik su usklađeni; {status}, {progress}. Ovo nije aplikacioni acceptance dokaz.")
    if "--self-test" in sys.argv:
        self_test(state, workflow)


if __name__ == "__main__":
    try:
        main()
    except (ValueError, KeyError, TypeError, OSError, yaml.YAMLError) as error:
        sys.exit(f"Provjera nije prošla: {error}")
