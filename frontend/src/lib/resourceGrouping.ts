import type { Resource, ResourceType } from "../api/types";

export function buildTypeNameMap(resourceTypes: ResourceType[]): Map<string, string> {
  return new Map(resourceTypes.map((t) => [t.id, t.name]));
}

export function filterByResourceType(resources: Resource[], typeId: string | null): Resource[] {
  return typeId === null ? resources : resources.filter((r) => r.resourceTypeId === typeId);
}

export function groupByTypeName(
  resources: Resource[],
  typeNameById: Map<string, string>
): Map<string, Resource[]> {
  const grouped = new Map<string, Resource[]>();
  for (const resource of resources) {
    const key = typeNameById.get(resource.resourceTypeId) ?? "Övrigt";
    const list = grouped.get(key) ?? [];
    list.push(resource);
    grouped.set(key, list);
  }
  return grouped;
}
