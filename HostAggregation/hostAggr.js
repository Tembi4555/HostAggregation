function prepareToJoin(segments) {
  const res = [];
  const includedValuesLength = segments.filter((item) => item.type == "incl").length;
  segments.forEach(element => {
    if (element.type === "incl") {
      const start = {num: element.start, flag: 1};
      const end = {num: element.end, flag: -1};
      res.push(start, end);
    }
    if (element.type === "excl") {
      const start = {num: element.start, flag: -includedValuesLength - 1};
      const end = {num: element.end, flag: includedValuesLength + 1};
      res.push(start, end);
    }
  });
  return res.sort((a, b) => a.num - b.num);
}

function joinSegments(inputSegments) {
  const segments = prepareToJoin(inputSegments);
  let trigger = 0;
  const res = [];
  let obj = {};
  for (let i = 0; i < segments.length; i++) {
    trigger += segments[i].flag;
    if (trigger > 0) {
      if (!obj.hasOwnProperty("start")) {
        obj.start = segments[i].num;  
      }
    }
    if (trigger <= 0) {
      obj.end = segments[i].num;
      obj.type = "incl";
      if (obj.hasOwnProperty("start")) {
        res.push(obj);
      }
      obj = {};
    }
  }
  return res;
}

function joinSegmentsWithOrder(inputSegments) {
  const candidateFirst = inputSegments.shift();
  const candidateSecond = inputSegments.shift();
  let candidates = [candidateFirst, candidateSecond];
  const isEveryExclude = (item) => item.type === "excl";
  if (!candidates.every(isEveryExclude)) {
    candidates = joinSegments(candidates);
  }
  while (inputSegments.length) {
    let currentCundidate = inputSegments.shift();
    candidates = [...candidates, currentCundidate];
    if (!candidates.every(isEveryExclude)) {
      candidates = joinSegments(candidates);
    }
    // console.log(candidates);
  }
  if (candidates.every(isEveryExclude)) {
    return [];
  }
  return candidates;
}

function main(segments) {
  //const joined = joinSegments(segments);
  //console.log(joined);
  const joinedWithOrder = joinSegmentsWithOrder(segments);
  console.log(joinedWithOrder);
}

testSegments = [
  // {type: "incl", start: 10, end: 20},
  // {type: "excl", start: -5, end: 15},
  // {type: "incl", start: -10, end: 20},
  // {type: "incl", start: 40, end: 100},
  // {type: "incl", start: -100, end: -30},
  // {type: "excl", start: 10, end: 15}
  {type: "excl", start: 10, end: 20},
  {type: "excl", start: 30, end: 55},
  {type: "excl", start: 50, end: 60},
  {type: "excl", start: 70, end: 80},
  {type: "incl", start: 15, end: 75},
  {type: "incl", start: 10, end: 15}
];

main(testSegments);